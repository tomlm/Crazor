using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Diag = System.Diagnostics;
using Crazor.Attributes;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Crazor.Interfaces;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Bot.Schema.Teams;

namespace Crazor
{
    /// <summary>
    /// CardApp is the base class of a card application
    /// </summary>
    /// <remarks>
    /// Derive from this class and add member variables with [SharedMemory] or [SessionMemory] attributes to control persistance.
    /// 
    /// LoadState() and SaveState() will be called by the CardBot class automatically.
    /// </remarks>
    public class CardApp
    {
        private static XmlSerializer _cardSerializer = new XmlSerializer(typeof(AdaptiveCard));
        private static XmlWriterSettings _settings = new XmlWriterSettings()
        {
            Encoding = new UnicodeEncoding(false, false), // no BOM in a .NET string
            Indent = true,
        };

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CardApp(IServiceProvider services)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            ArgumentNullException.ThrowIfNull(services);

            this.Services = services;
            this.CallStack = new List<CardViewState>()
            {
                new CardViewState(Constants.DEFAULT_VIEW, null)
            };
            this.CardType = this.GetType().Name;
            this.Name = CardType.EndsWith("App") ? CardType.Substring(0, CardType.Length - 3) : CardType;
        }

        /// <summary>
        /// Name of the App.
        /// </summary>
        [JsonIgnore]
        public string CardType { get; private set; }

        [JsonIgnore]
        public string Name { get; set; }

        /// <summary>
        /// Instance Id for the card.
        /// </summary>
        [JsonIgnore]
        public string? SharedId { get; set; }

        /// <summary>
        /// Session Id for the card
        /// </summary>
        [JsonIgnore]
        public string? SessionId { get; private set; }

        /// <summary>
        /// The activity we are processing.
        /// </summary>
        [JsonIgnore]
        public Activity? Activity { get; set; }

        /// <summary>
        /// The action we are processing.
        /// </summary>
        [JsonIgnore]
        public AdaptiveCardInvokeAction? Action { get; set; }

        [JsonIgnore]
        public IServiceProvider Services { get; set; }

        /// <summary>
        /// Navigation stack
        /// </summary>
        [SessionMemory]
        [JsonIgnore]
        public List<CardViewState> CallStack { get; set; }

        [JsonIgnore]
        public string CurrentCard => CallStack.First().Name;

        [JsonIgnore]
        public ICardView CurrentView { get; private set; }

        [JsonIgnore]
        public CardResult? LastResult { get; set; }

        /// <summary>
        /// Messages to add to the card
        /// </summary>
        [JsonIgnore]
        public List<BannerMessage> BannerMessages { get; private set; } = new List<BannerMessage>();

        [JsonIgnore]
        public Dictionary<string, AdaptiveElement>? Stylesheet { get; set; }

        [JsonIgnore]
        public bool IsTaskModule { get; set; } = false;

        [JsonIgnore]
        public TaskModuleStatus TaskModuleStatus { get; set; }

        [JsonIgnore]
        public MessagingExtensionAction MessageExtensionAction { get; set; }

        public virtual async Task<AdaptiveCard> OnActionExecuteAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var result = await OnActionExecuteAsync(cancellationToken);

            if (result is AdaptiveCard adaptiveCard && turnContext.Activity.ChannelId == Channels.Msteams)
            {
                // we need to add refresh userids
                var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
                var teamsMembers = await connectorClient.Conversations.GetConversationPagedMembersAsync(turnContext.Activity.Conversation.Id, 60, cancellationToken: cancellationToken);
                adaptiveCard.Refresh.UserIds = teamsMembers.Members.Select(member => $"8:orgid:{member.AadObjectId}").ToList();
            }
            return result;
        }

        /// <summary>
        /// Handle action
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual async Task<AdaptiveCard> OnActionExecuteAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(this.Action);
            ArgumentNullException.ThrowIfNull(CurrentView);
            ArgumentNullException.ThrowIfNull(this.Action);
            Diag.Trace.WriteLine($"------- OnAction({this.Action.Verb})-----");

            // Load stylesheet
            if (Stylesheet == null)
            {
                var stylesheetPath = Path.Combine(Environment.CurrentDirectory, $"Cards/{Name}/Stylesheet.cshtml");
                if (File.Exists(stylesheetPath))
                {
                    var xml = await File.ReadAllTextAsync(stylesheetPath, cancellationToken);
                    xml = $"<Card Version=\"1.6\">\n{xml}\n</Card>";
                    var card = (AdaptiveCard)_cardSerializer!.Deserialize(XmlReader.Create(new StringReader(xml!)))!;
                    Stylesheet = card.Body.ToDictionary(el => $"{el.Type}.{el.Id}", StringComparer.OrdinalIgnoreCase);
                }
            }

            try
            {
                if (!String.IsNullOrEmpty(this.Action.Verb))
                {
                    var state = this.CallStack[0];
                    var cardView = this.CurrentView;

                    await this.CurrentView.OnVerbAsync(this.Action, cancellationToken);
                    if (LastResult != null)
                    {
                        await this.CurrentView.OnCardResumeAsync(LastResult, cancellationToken);
                    }

                    state.Model = cardView.GetModel();
                    if (state.Model is CardApp)
                    {
                        state.Model = null;
                    }
                }
            }
            catch (Exception err)
            {
                // if we fail in verb we add message banner to template.
                AddBannerMessage(err.Message, AdaptiveContainerStyle.Attention);
            }

            AdaptiveCard? outboundCard;
            try
            {
                outboundCard = await CurrentView.BindView(Services);
                ArgumentNullException.ThrowIfNull(outboundCard);
            }
            catch (XmlException xerr)
            {
                Diag.Debug.WriteLine(xerr.Message);

                // if we fail to bind to the template we send empty card with message.
                outboundCard = new AdaptiveCard("1.3");
                AddBannerMessage(xerr.Message.Replace("\n", "\n\n"), AdaptiveContainerStyle.Attention);
            }
            catch (Exception err)
            {
                Diag.Debug.WriteLine(err.Message);

                // if we fail to bind to the template we send empty card with message.
                outboundCard = new AdaptiveCard("1.3");
                var innerMessage = err.InnerException?.Message ?? String.Empty;
                AddBannerMessage($"\n{err.Message.Replace("\n", "\n\n")}\n{innerMessage}", AdaptiveContainerStyle.Attention);
            }

            await ApplyCardModificationsAsync(outboundCard, cancellationToken);

            return outboundCard;
        }

        /// <summary>
        /// Handle search 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual async Task<AdaptiveCardInvokeResponse> OnSearchInvokeAsync(SearchInvoke searchInvoke, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(CurrentView);
            Diag.Trace.WriteLine($"------- OnSearch({searchInvoke.Dataset}, {searchInvoke.QueryText})-----");

            var choices = await CurrentView.OnSearchChoicesAsync(searchInvoke, Services);

            return new AdaptiveCardInvokeResponse()
            {
                StatusCode = 200,
                Type = "application/vnd.microsoft.search.searchResponse",
                Value = JObject.FromObject(new { results = choices })
            };
        }

        /// <summary>
        /// Navigate to card by name passing optional model
        /// </summary>
        /// <param name="cardName"></param>
        /// <param name="model"></param>
        public void ShowCard(string cardName, object? model = null)
        {
            this.CallStack.Insert(0, new CardViewState(cardName, model));
            this.CurrentView = View(cardName, model);
        }

        /// <summary>
        /// Replace current card with a different card by name passing optional model
        /// </summary>
        /// <param name="cardName">card to switch to</param>
        /// <param name="model">model to pass card</param>
        public void ReplaceCard(string cardName, object? model = null)
        {
            this.CallStack[0] = new CardViewState(cardName, model);
            this.CurrentView = View(cardName, model);
        }

        public void CloseCard(CardResult? result = null)
        {
            var lastCard = this.CurrentCard;
            this.LastResult = result;

            if (this.CallStack.Any())
            {
                this.CallStack.RemoveAt(0);
            }

            if (!this.CallStack.Any())
            {
                this.CallStack.Insert(0, new CardViewState(Constants.DEFAULT_VIEW));
            }

            this.CurrentView = View(this.CurrentCard, this.CallStack[0].Model);
        }

        public void CloseTaskModule(TaskModuleStatus status)
        {
            this.TaskModuleStatus = status;
        }

        /// <summary>
        /// Add a banner message 
        /// </summary>
        /// <param name="text">text to show</param>
        /// <param name="style">color to use</param>
        public void AddBannerMessage(string text, AdaptiveContainerStyle style = AdaptiveContainerStyle.Accent)
        {
            BannerMessages.Add(new BannerMessage() { Text = text, Style = style });
        }

        public virtual string GetRoute()
        {
            UriBuilder uri = new UriBuilder();

            var viewRoute = this.CurrentView!.GetRoute();
            if (!viewRoute.StartsWith('/'))
            {
                if (viewRoute.Length > 0)
                    uri.Path = $"/Cards/{this.Name}/{this.CurrentView.Name}/{viewRoute}";
                else if (this.CurrentView.Name != Constants.DEFAULT_VIEW)
                    uri.Path = $"/Cards/{this.Name}/{this.CurrentView.Name}";
                else
                    uri.Path = $"/Cards/{this.Name}";
            }

            if (!String.IsNullOrEmpty(this.SharedId))
                uri.Query = $"id={this.SharedId}";
            return uri.Uri.PathAndQuery;
        }

        /// <summary>
        /// Override this to set the shared Id when known.
        /// </summary>
        /// <returns></returns>
        public virtual string GetSharedId() => null;

        /// <summary>
        /// Load state from storage
        /// </summary>
        /// <param name="storage"></param>
        /// <returns></returns>
        public async virtual Task LoadAppAsync(string? sharedId, string? sessionId, Activity activity, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(activity);
            this.SharedId = sharedId ?? GetSharedId();
            this.SessionId = sessionId ?? Utils.GetNewId();
            this.Activity = activity;
            var invoke = JToken.FromObject(activity.Value).ToObject<AdaptiveCardInvokeValue>();
            ArgumentNullException.ThrowIfNull(invoke);
            this.Action = invoke.Action;

            var storage = this.Services.GetRequiredService<IStorage>();
            var sharedKey = (SharedId != null) ? GetKey(SharedId) : null;
            var sessionKey = (SessionId != null) ? GetKey(SessionId) : null;

            var keys = new List<string>() { };
            if (sharedKey != null)
            {
                keys.Add(sharedKey);
            }

            if (sessionKey != null)
            {
                keys.Add(sessionKey);
            }

            var state = await storage.ReadAsync(keys.ToArray(), cancellationToken);
            if (sharedKey != null && state.ContainsKey(sharedKey))
            {
                SetScopedMemory<SharedMemoryAttribute>((JObject)state[sharedKey]);
            }

            if (sessionKey != null && state.ContainsKey(sessionKey))
            {
                SetScopedMemory<SessionMemoryAttribute>((JObject)state[sessionKey]);
            }

            if (Action?.Verb == Constants.LOADROUTE_VERB)
            {
                var loadRoute = JObject.FromObject(Action.Data).ToObject<LoadRouteModel>();
                if (loadRoute != null && this.CurrentCard != loadRoute.View)
                {
                    ShowCard(loadRoute.View!);
                    return;
                }
            }

            // load current card.
            var cardView = View(this.CurrentCard, this.CallStack[0].Model);

            // NOTE: if the current card navigates to another card
            // then CurrentView will be non null, but if it doesn't
            // then we need to set it.
            if (this.CurrentView == null)
            {
                this.CurrentView = cardView;
            }
        }

        /// <summary>
        /// Save state from storage
        /// </summary>
        /// <param name="storage"></param>
        /// <returns></returns>
        public async virtual Task SaveAppAsync(CancellationToken cancellationToken)
        {
            var storage = this.Services.GetRequiredService<IStorage>();
            var data = new Dictionary<string, object>();
            if (SharedId != null)
            {
                data.Add(GetKey(SharedId), GetScopedMemory<SharedMemoryAttribute>());
            }

            if (SessionId != null)
            {
                data.Add(GetKey(SessionId), GetScopedMemory<SessionMemoryAttribute>());
            }
            await storage.WriteAsync(data, cancellationToken);
        }

        /// <summary>
        /// HasView
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns>true or false</returns>
        public bool HasView(string viewName)
        {
            var razorEngine = this.Services.GetRequiredService<IRazorViewEngine>();
            var viewPath = $"/Cards/{Name}/{viewName}.cshtml";
            var viewResult = razorEngine.GetView(Environment.CurrentDirectory, viewPath, false);
            return (viewResult?.View != null);
        }


        /// <summary>
        /// Given view name, return the IView object
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns>view</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ICardView View(string viewName, object? model = null)
        {
            var razorEngine = this.Services.GetRequiredService<IRazorViewEngine>();
            var viewPath = $"/Cards/{Name}/{viewName}.cshtml";
            var viewResult = razorEngine.GetView(Environment.CurrentDirectory, viewPath, false);
            if (viewResult?.View == null)
            {
                throw new ArgumentNullException($"{viewName} does not match any available view");
            }

            ICardView cardView = (ICardView)((RazorView)viewResult.View).RazorPage;
            cardView.RazorView = viewResult.View;
            cardView.UrlHelper = this.Services.GetRequiredService<IUrlHelper>();
            cardView.App = this;
            cardView.Name = viewName;
            ITempDataProvider tempDataProvider;
            ActionContext actionContext;
            var httpContext = new DefaultHttpContext { RequestServices = Services };
            actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            tempDataProvider = Services.GetRequiredService<ITempDataProvider>();
            var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };
            var viewContext = new ViewContext(actionContext, viewResult.View, viewDictionary, new TempDataDictionary(actionContext.HttpContext, tempDataProvider), new StringWriter(), new HtmlHelperOptions());
            cardView.ViewContext = viewContext;
            cardView.OnLoadCardContext(viewContext);
            return cardView;
        }

        private async Task ApplyCardModificationsAsync(AdaptiveCard outboundCard, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(this.Activity);
            ArgumentNullException.ThrowIfNull(this.Action);

            AddRefresh(outboundCard);

            if (this.Activity.ChannelId != Channels.Msteams)
            {
                AddSystemMenu(outboundCard);
            }

            AddMessageBanner(outboundCard);

            // add session data to outbound card
            await AddSessionDataToAdaptiveCardAsync(outboundCard, cancellationToken);

#pragma warning disable CS0618 // Type or member is obsolete
            if (CurrentCard != Constants.DEFAULT_VIEW)
            {
                outboundCard.Title = $"{this.Name} - {CurrentCard}";
            }
            else
            {
                outboundCard.Title = $"{this.Name}";
            }
#pragma warning restore CS0618 // Type or member is obsolete

            outboundCard.AdditionalProperties["url"] = GetRoute();
        }

        private async Task<SessionData> AddSessionDataToAdaptiveCardAsync(AdaptiveCard outboundCard, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(this.SessionId);

            var sessionData = new SessionData
            {
                App = this.CardType,
                SharedId = this.SharedId,
                SessionId = this.SessionId
            };
            var sessionDataToken = sessionData.ToString();

            var encryptionProvider = this.Services.GetService<IEncryptionProvider>();
            if (encryptionProvider != null)
            {
                sessionDataToken = await encryptionProvider.EncryptAsync(sessionDataToken.ToString(), cancellationToken);
            }

            foreach (var action in outboundCard.GetElements<AdaptiveExecuteAction>())
            {
                if ((action.Data == null) ||
                    (action.Data is string text && string.IsNullOrWhiteSpace(text)))
                {
                    action.Data = new JObject();
                }
                var data = (JObject)action.Data;
                data[Constants.SESSIONDATA_KEY] = JToken.FromObject(sessionDataToken);
                if (action.Id != null)
                    data[Constants.IDDATA_KEY] = action.Id;
            }

            foreach (var action in outboundCard.GetElements<AdaptiveSubmitAction>())
            {
                // YAML authoring errors - "data:" without children properties is an empty string
                if (action.Data == null || (action.Data is string text && string.IsNullOrWhiteSpace(text)))
                {
                    action.Data = new JObject();
                }

                var data = (JObject)action.Data;
                data[Constants.SESSIONDATA_KEY] = JToken.FromObject(sessionDataToken);
                if (action.Id != null)
                    data[Constants.IDDATA_KEY] = action.Id;
            }

            foreach (var choiceSet in outboundCard.GetElements<AdaptiveChoiceSetInput>())
            {
                if (choiceSet.DataQuery != null)
                {
                    choiceSet.DataQuery.Dataset = $"{sessionDataToken}{AdaptiveDataQuery.Separator}{choiceSet.DataQuery.Dataset}";
                }
            }
            return sessionData;
        }

        private static void AddRefresh(AdaptiveCard outboundCard)
        {
            var refresh = new AdaptiveExecuteAction()
            {
                Title = "Refresh",
                Verb = Constants.SHOW_VERB,
                IconUrl = "https://powercardbot.azurewebsites.net/refresh.png",
                AssociatedInputs = AdaptiveAssociatedInputs.None
                //AdditionalProperties = new SerializableDictionary<string, object>() { { "mode", "secondary" } }
            };
            outboundCard.Refresh = new AdaptiveRefresh() { Action = refresh };
        }

        private void AddMessageBanner(AdaptiveCard outboundCard)
        {
            if (this.BannerMessages.Any())
            {
#if MSTEAMS_BANNER
                if (this.Activity.ChannelId == Channels.Msteams && this.Temp.message != null)
                {
                    return Task.FromResult(new AdaptiveCardInvokeResponse()
                    {
                        StatusCode = 200,
                        Type = "text/plain",
                        Value = (string)this.Temp.message
                    });
                }
#else
                int iMessage = 0;
                foreach (var message in this.BannerMessages)
                {
                    var banner = new AdaptiveColumnSet()
                    {
                        Id = $"messageBanner{iMessage}",
                        Style = message.Style,
                        Spacing = AdaptiveSpacing.None,
                        Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width = AdaptiveColumnWidth.Stretch,
                                Spacing = AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock((string)message.Text) { Wrap = true }
                                }
                            },
                            new AdaptiveColumn()
                            {
                                Width = AdaptiveColumnWidth.Auto,
                                Spacing = AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock("🗙")
                                },
                                SelectAction = new AdaptiveToggleVisibilityAction()
                                {
                                    Title = "Close",
                                    TargetElements = new List<AdaptiveTargetElement>() { new AdaptiveTargetElement($"messageBanner{iMessage}") }
                                }
                            }
                        }
                    };
                    outboundCard.Body.Insert(0, banner);
                    iMessage++;
                }
#endif
            }
        }


        private void AddSystemMenu(AdaptiveCard outboundCard)
        {
            if (outboundCard.Body.Any())
            {
                outboundCard.Body.First().Separator = false;
            }

            var ellipsis = new AdaptiveColumnSet()
            {
                Separator = false,
                Spacing = AdaptiveSpacing.None,
                Columns = new List<AdaptiveColumn>()
                {
                    new AdaptiveColumn()
                    {
                        Width = AdaptiveColumnWidth.Stretch,
                        Spacing = AdaptiveSpacing.None,
                    },
                    new AdaptiveColumn()
                    {
                        Spacing = AdaptiveSpacing.None,
                        Width = AdaptiveColumnWidth.Auto,
                        VerticalContentAlignment = AdaptiveVerticalContentAlignment.Bottom,
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveImage()
                            {
                                Url = "https://powercardbot.azurewebsites.net/ellipsis.png",
                                Width = "16px",
                                SelectAction = new AdaptiveToggleVisibilityAction()
                                {
                                    TargetElements = new List<AdaptiveTargetElement>()
                                    {
                                        new AdaptiveTargetElement()
                                        {
                                             ElementId = "systemMenu"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var systemMenu = new AdaptiveActionSet()
            {
                Id = "systemMenu",
                IsVisible = false
            };
            outboundCard.Body.Insert(0, ellipsis);

            if (HasView(Constants.ABOUT_VIEW))
            {
                systemMenu.Actions.Add(new AdaptiveExecuteAction()
                {
                    Title = Constants.ABOUT_VIEW,
                    Verb = Constants.ABOUT_VIEW,
                    IconUrl = "https://powercardbot.azurewebsites.net/about.png",
                    AssociatedInputs = AdaptiveAssociatedInputs.None
                    //   AdditionalProperties = new SerializableDictionary<string, object>() { { "mode", "secondary" } }
                });
            }

            if (outboundCard.Refresh != null)
            {
                systemMenu.Actions.Add(outboundCard.Refresh.Action);
            }

            if (this.HasView(Constants.SETTINGS_VIEW))
            {
                systemMenu.Actions.Add(new AdaptiveExecuteAction()
                {
                    Title = Constants.SETTINGS_VIEW,
                    Verb = Constants.SETTINGS_VIEW,
                    IconUrl = "https://powercardbot.azurewebsites.net/settings.png",
                    AssociatedInputs = AdaptiveAssociatedInputs.None
                    //AdditionalProperties = new SerializableDictionary<string, object>() { { "mode", "secondary" } }
                });
            }

            outboundCard.Body.Insert(1, systemMenu);
        }

        private object GetScopedMemory<MemoryAttributeT>()
                where MemoryAttributeT : Attribute
        {
            lock (this)
            {
                dynamic memory = new JObject();
                foreach (var property in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(MemoryAttributeT))))
                {
                    var val = property.GetValue(this);
                    memory[property.Name] = val != null ? JToken.FromObject(val) : null;
                }

                return memory;
            }
        }

        private void SetScopedMemory<MemoryAttributeT>(JObject memory)
            where MemoryAttributeT : Attribute
        {
            lock (this)
            {
                foreach (var property in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(MemoryAttributeT))))
                {
                    if (memory.ContainsKey(property.Name))
                    {
                        var value = memory[property.Name];
                        if (value != null)
                        {
                            property.SetValue(this, value.ToObject(property.PropertyType));
                        }
                    }
                }
            }
        }

        private string GetKey(string? key) => $"{CardType}-{key}";

        /// <summary>
        /// Parse a /cards/{app}/{view}{path} into parts.
        /// </summary>
        /// <param name="localPath"></param>
        /// <param name="app"></param>
        /// <param name="sharedId"></param>
        /// <param name="view"></param>
        /// <param name="path"></param>
        public static void ParseUri(Uri uri, out string app, out string? sharedId, out string? view, out string path)
        {
            sharedId = null;
            var parts = uri.LocalPath.Trim('/').Split('/');
            app = parts[1] + "App";
            // sharedId = (parts.Length > 2) ? parts[2] : null;
            view = (parts.Length > 2) ? parts[2] : null;
            path = String.Join('/', parts.Skip(3).ToArray());
            if (!String.IsNullOrEmpty(uri.Query))
            {
                var dict = QueryHelpers.ParseQuery(uri.Query);
                if (dict.TryGetValue("id", out var values))
                {
                    sharedId = values.Single();
                }
            }
        }

    }
}
