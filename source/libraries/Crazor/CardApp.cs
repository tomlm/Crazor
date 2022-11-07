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
using Crazor.Interfaces;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Neleus.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Mvc.ViewEngines;

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

        protected IConfiguration _configuration;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CardApp(IServiceProvider services)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            ArgumentNullException.ThrowIfNull(services);

            this.Services = services;
            this._configuration = services.GetRequiredService<IConfiguration>();
            this.CallStack = new List<CardViewState>()
            {
                new CardViewState(Constants.DEFAULT_VIEW, null)
            };
            this.CardType = this.GetType().Name;
            this.Name = CardType.EndsWith("App") ? CardType.Substring(0, CardType.Length - 3) : CardType;
        }

        /// <summary>
        /// Typoe of the App.
        /// </summary>
        public string CardType { get; private set; }

        /// <summary>
        /// Name of the app
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// IconUrl to use for the card.
        /// </summary>
        public string IconUrl { get; set; }

        /// <summary>
        /// <summary>
        /// Instance Id for the card.
        /// </summary>
        public string? SharedId { get; set; }

        /// Session Id for the card
        /// </summary>
        public string? SessionId { get; private set; }

        /// <summary>
        /// The activity we are processing.
        /// </summary>
        public Activity? Activity { get; set; }

        /// <summary>
        /// The default view to render
        /// </summary>
        public string DefaultView { get; set; } = Constants.DEFAULT_VIEW;

        /// <summary>
        /// The action we are processing.
        /// </summary>
        public AdaptiveCardInvokeAction? Action { get; set; }

        public IServiceProvider Services { get; set; }

        /// <summary>
        /// Navigation stack
        /// </summary>
        [SessionMemory]
        public List<CardViewState> CallStack { get; set; }

        public string CurrentCard => CallStack.First().Name;

        public ICardView CurrentView { get; private set; }

        public CardResult? LastResult { get; set; }

        /// <summary>
        /// Messages to add to the card
        /// </summary>
        public List<BannerMessage> BannerMessages { get; private set; } = new List<BannerMessage>();

        public Dictionary<string, AdaptiveElement>? Stylesheet { get; set; }

        public bool IsTaskModule { get; set; } = false;

        public bool IsTabModule => this.Activity.ChannelId == Channels.Msteams && this.Activity.Conversation.Id.StartsWith("tab:");

        public TaskModuleAction TaskModuleStatus { get; set; }

        public MessagingExtensionAction MessageExtensionAction { get; set; }

        public virtual async Task<AdaptiveCard> OnActionExecuteAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var result = await OnActionExecuteAsync(cancellationToken);

            if (result is AdaptiveCard adaptiveCard && turnContext.Activity.ChannelId == Channels.Msteams &&
                !turnContext.Activity.Conversation.Id.StartsWith("tab:"))
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
            if (this.Action.Verb == null)
            {
                this.Action.Verb = Constants.SHOWVIEW_VERB;
            }
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
                    await this.CurrentView.OnActionAsync(this.Action, cancellationToken);
                    if (LastResult != null)
                    {
                        await this.CurrentView.OnResumeViewAsync(LastResult, cancellationToken);
                    }

                    SaveCardState(this.CallStack[0], this.CurrentView);
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
                outboundCard = await CurrentView.BindView(cancellationToken);
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

            var choices = await CurrentView.OnSearchChoicesAsync(searchInvoke, cancellationToken);

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
        public void ShowView(string cardName, object? model = null)
        {
            SaveCardState(this.CallStack[0], this.CurrentView);
            var cardState = new CardViewState(cardName, model);
            CallStack.Insert(0, cardState);
            Action!.Verb = Constants.SHOWVIEW_VERB;
            LoadView(cardState);
        }

        /// <summary>
        /// Replace current card with a different card by name passing optional model
        /// </summary>
        /// <param name="cardName">card to switch to</param>
        /// <param name="model">model to pass card</param>
        public void ReplaceView(string cardName, object? model = null)
        {
            var cardState = new CardViewState(cardName, model);
            this.CallStack[0] = cardState;
            Action!.Verb = Constants.SHOWVIEW_VERB;
            LoadView(cardState);
        }

        public void CloseView(CardResult? result = null)
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

            Action!.Verb = Constants.SHOWVIEW_VERB;
            LoadView(this.CallStack[0]);
        }

        public void CloseTaskModule(TaskModuleAction status)
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
        public virtual string? GetSharedId() => null;

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
                    ShowView(loadRoute.View!);
                    return;
                }
            }

            // load current card.
            LoadView(this.CallStack[0]);
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
        public void LoadView(CardViewState cardState)
        {
            var razorEngine = this.Services.GetRequiredService<IRazorViewEngine>();
            var viewPath = $"/Cards/{Name}/{cardState.Name}.cshtml";
            var viewResult = razorEngine.GetView(Environment.CurrentDirectory, viewPath, false);
            IView view;
            ICardView cardView;
            if (viewResult?.View != null)
            {
                view = viewResult?.View!;
                cardView = (ICardView)((RazorView)viewResult.View).RazorPage;
                cardView.RazorView = viewResult.View;
            }
            else
            {
                // This is a non-cshtml view, let's see if we can construct it.
                cardView = this.Services.GetByName<ICardView>(cardState.Name);
                view = new ViewStub() ;
                ArgumentNullException.ThrowIfNull(cardView);
            }

            cardView.UrlHelper = this.Services.GetRequiredService<IUrlHelper>();
            cardView.App = this;
            cardView.Name = cardState.Name;

            // rester card SessionMemory properties
            LoadCardState(cardState, cardView);

            ITempDataProvider tempDataProvider;
            ActionContext actionContext;
            var httpContext = new DefaultHttpContext { RequestServices = Services };
            actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            tempDataProvider = Services.GetRequiredService<ITempDataProvider>();
            var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = cardState.Model
            };

            var viewContext = new ViewContext(actionContext, view, viewDictionary, new TempDataDictionary(actionContext.HttpContext, tempDataProvider), new StringWriter(), new HtmlHelperOptions());
            cardView.ViewContext = viewContext;
            this.CurrentView = cardView;
            this.CurrentView.LoadState(cardState);
        }

        private async Task ApplyCardModificationsAsync(AdaptiveCard outboundCard, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(this.Activity);
            ArgumentNullException.ThrowIfNull(this.Action);

            AddRefresh(outboundCard);

            AddMessageBanner(outboundCard);

            AddSystemMenu(outboundCard);

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

        public SessionData GetSessionData()
        {
            return new SessionData()
            {
                App = this.CardType,
                SharedId = this.SharedId,
                SessionId = this.SessionId
            };
        }

        public async Task<string> GetSessionDataToken(CancellationToken cancellationToken)
        {
            var sessionData = GetSessionData();
            var sessionDataToken = sessionData.ToString();

            var encryptionProvider = this.Services.GetService<IEncryptionProvider>();
            if (encryptionProvider != null)
            {
                sessionDataToken = await encryptionProvider.EncryptAsync(sessionDataToken.ToString(), cancellationToken);
            }
            return sessionDataToken;
        }

        private async Task AddSessionDataToAdaptiveCardAsync(AdaptiveCard outboundCard, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(this.SessionId);

            var sessionDataToken = await GetSessionDataToken(cancellationToken);

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
        }

        private static void AddRefresh(AdaptiveCard outboundCard)
        {
            var refresh = new AdaptiveExecuteAction()
            {
                Title = "Refresh",
                Verb = Constants.SHOWVIEW_VERB,
                IconUrl = "https://powercardbot.azurewebsites.net/refresh.png",
                AssociatedInputs = AdaptiveAssociatedInputs.None,
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
            var systemMenu = new AdaptiveActionSet()
            {
                Id = "systemMenu",
                IsVisible = false,
                Separator = true
            };

            if (Activity.ChannelId == _configuration.GetValue<Uri>("HostUri").Host && outboundCard.Refresh?.Action != null)
            {
                outboundCard.Refresh.Action.Mode = AdaptiveActionMode.Secondary;
                systemMenu.Actions.Add(outboundCard.Refresh.Action);
            }

            if (HasView(Constants.ABOUT_VIEW) && this.CurrentCard != Constants.ABOUT_VIEW)
            {
                systemMenu.Actions.Add(new AdaptiveExecuteAction()
                {
                    Title = Constants.ABOUT_VIEW,
                    Verb = Constants.ABOUT_VIEW,
                    IconUrl = "https://powercardbot.azurewebsites.net/about.png",
                    AssociatedInputs = AdaptiveAssociatedInputs.None,
                    Mode = AdaptiveActionMode.Secondary
                });
            }

            if (this.HasView(Constants.SETTINGS_VIEW) && this.CurrentCard != Constants.SETTINGS_VIEW)
            {
                systemMenu.Actions.Add(new AdaptiveExecuteAction()
                {
                    Title = Constants.SETTINGS_VIEW,
                    Verb = Constants.SETTINGS_VIEW,
                    IconUrl = "https://powercardbot.azurewebsites.net/settings.png",
                    AssociatedInputs = AdaptiveAssociatedInputs.None,
                    Mode = AdaptiveActionMode.Secondary
                });
            }

            if (this.Activity.ChannelId == _configuration.GetValue<Uri>("HostUri").Host)
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
                            Width = AdaptiveColumnWidth.Auto,
                            VerticalContentAlignment = AdaptiveVerticalContentAlignment.Center,
                            Items = new List<AdaptiveElement>()
                            {
                                new AdaptiveImage(new Uri(_configuration.GetValue<Uri>("HostUri"), _configuration.GetValue<string>("BotIcon") ?? "/images/boticon.png").AbsoluteUri)
                                {
                                    Size= AdaptiveImageSize.Small
                                }
                            }
                        },
                        new AdaptiveColumn()
                        {
                            Width = AdaptiveColumnWidth.Stretch,
                            VerticalContentAlignment = AdaptiveVerticalContentAlignment.Center,
                            Items = new List<AdaptiveElement>()
                            {
                                new AdaptiveTextBlock(_configuration.GetValue<string>("BotName")) { Size= AdaptiveTextSize.Small }
                            }
                        },
                        new AdaptiveColumn()
                        {
                            Width = AdaptiveColumnWidth.Auto,
                            VerticalContentAlignment = AdaptiveVerticalContentAlignment.Bottom,
                            Items = new List<AdaptiveElement>()
                            {
                                new AdaptiveRichTextBlock()
                                {
                                    Inlines = new List<AdaptiveInline>()
                                    {
                                        new AdaptiveTextRun("…")
                                        {
                                            Size = AdaptiveTextSize.Large,
                                            Weight = AdaptiveTextWeight.Bolder,
                                            SelectAction = new AdaptiveToggleVisibilityAction()
                                            {
                                                TargetElements = new List<AdaptiveTargetElement>()
                                                {
                                                    new AdaptiveTargetElement() { ElementId = "systemMenu"}
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

                outboundCard.Body.Insert(0, ellipsis);

                foreach (var action in systemMenu.Actions)
                {
                    action.Mode = AdaptiveActionMode.Primary;
                }

                outboundCard.Body.Insert(1, systemMenu);
                outboundCard.Body.Insert(2, new AdaptiveContainer() { Separator = true, Spacing = AdaptiveSpacing.None, Id = "systemMenuBorder" });
            }
            else
            {
                foreach (var action in systemMenu.Actions)
                {
                    outboundCard.Actions.Add(action);
                }
            }
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

        private static void LoadCardState(CardViewState cardState, ICardView cardView)
        {
            foreach (var property in cardView.GetType().GetProperties()
                                                        .Where(prop => prop.GetCustomAttribute<SessionMemoryAttribute>() != null))
            {
                if (cardState.SessionMemory.TryGetValue(property.Name, out var val))
                {
                    cardView.SetTargetProperty(property, val);
                }
            }
        }



        private static void SaveCardState(CardViewState state, ICardView cardView)
        {
            if (cardView != null)
            {
                // capture all properties on CardView which are not on base type and not ignored.
                foreach (var property in cardView.GetType().GetProperties()
                                                            .Where(prop => prop.GetCustomAttribute<SessionMemoryAttribute>() != null))
                {
                    var val = property.GetValue(cardView);
                    if (val != null)
                    {
                        if (property.Name == "Model")
                        {
                            state.Model = val;
                        }
                        else
                        {
                            state.SessionMemory[property.Name] = JToken.FromObject(val);
                        }
                    }
                }
            }
        }

        private string GetKey(string? key) => $"{CardType}-{key}";

        /// <summary>
        /// Parse a /cards/{app}/{view}{path} into parts.
        /// </summary>
        /// <param name="uri">uri</param>
        /// <param name="turnContext">Turn context</param>
        /// <param name="app">app from the uri</param>
        /// <param name="sharedId">sharedId from the uri</param>
        public static void ParseUri(Uri uri, out string app, out string? sharedId, out string view, out string path)
        {
            sharedId = null;
            var parts = uri.LocalPath.Trim('/').Split('/');
            app = parts[1] + "App";
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

        private class ViewStub : IView
        {
            public string Path { get; set; } = string.Empty;

            public Task RenderAsync(ViewContext context)
            {
                return Task.CompletedTask;
            }
        }
    }
}
