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
using static Crazor.CardActivityHandler;

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
        public CardApp(IServiceProvider services)
        {
            ArgumentNullException.ThrowIfNull(services);

            this.Services = services;
            this.CallStack = new List<CardViewState>()
            {
                new CardViewState("Default", null)
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
        public string? ResourceId { get; private set; }

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
        public CardView? CurrentView { get; private set; }

        [JsonIgnore]
        public CardResult? LastResult { get; set; }

        /// <summary>
        /// Messages to add to the card
        /// </summary>
        [JsonIgnore]
        public List<BannerMessage> BannerMessages { get; private set; } = new List<BannerMessage>();

        /// <summary>
        /// Handle action
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual async Task<AdaptiveCardInvokeResponse> OnActionExecuteAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(this.Action);
            ArgumentNullException.ThrowIfNull(CurrentView);
            ArgumentNullException.ThrowIfNull(this.Action);
            Diag.Trace.WriteLine($"------- OnAction({this.Action.Verb})-----");
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

            return new AdaptiveCardInvokeResponse()
            {
                StatusCode = 200,
                Type = AdaptiveCard.ContentType,
                Value = outboundCard
            };
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

        public void ShowCard(string cardName, object? model = null)
        {
            this.CallStack.Insert(0, new CardViewState(cardName, model));
            this.CurrentView = View(cardName, model);
        }

        public void Close(CardResult? result = null)
        {
            var lastCard = this.CurrentCard;
            this.LastResult = result;

            if (this.CallStack.Any())
            {
                this.CallStack.RemoveAt(0);
            }

            if (!this.CallStack.Any())
            {
                this.CallStack.Insert(0, new CardViewState("Default"));
            }

            this.CurrentView = View(this.CurrentCard, this.CallStack[0].Model);
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

        /// <summary>
        /// Load state from storage
        /// </summary>
        /// <param name="storage"></param>
        /// <returns></returns>
        public async virtual Task LoadAppAsync(string? resourceId, string? sessionId, Activity activity, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(activity);
            this.ResourceId = resourceId;
            this.SessionId = sessionId ?? Utils.GetNewId();
            this.Activity = activity;
            var invoke = JToken.FromObject(activity.Value).ToObject<AdaptiveCardInvokeValue>();
            ArgumentNullException.ThrowIfNull(invoke);
            this.Action = invoke.Action;

            var storage = this.Services.GetRequiredService<IStorage>();
            var resourceKey = GetKey(ResourceId);
            var sessionKey = (SessionId != null) ? GetKey(SessionId) : null;

            var keys = new List<string>() { resourceKey };
            if (sessionKey != null)
            {
                keys.Add(sessionKey);
            }

            var state = await storage.ReadAsync(keys.ToArray(), cancellationToken);
            if (state.ContainsKey(resourceKey))
            {
                SetScopedMemory<SharedMemoryAttribute>((JObject)state[resourceKey]);
            }

            if (sessionKey != null && state.ContainsKey(sessionKey))
            {
                SetScopedMemory<SessionMemoryAttribute>((JObject)state[sessionKey]);
            }

            this.CurrentView = View(this.CurrentCard, this.CallStack[0].Model);
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
            if (ResourceId != null)
            {
                data.Add(GetKey(ResourceId), GetScopedMemory<SharedMemoryAttribute>());
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
        public CardView View(string viewName, object? model = null)
        {
            var razorEngine = this.Services.GetRequiredService<IRazorViewEngine>();
            var viewPath = $"/Cards/{Name}/{viewName}.cshtml";
            var viewResult = razorEngine.GetView(Environment.CurrentDirectory, viewPath, false);
            if (viewResult?.View == null)
            {
                throw new ArgumentNullException($"{viewName} does not match any available view");
            }

            CardView cardView = (CardView)((RazorView)viewResult.View).RazorPage;
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
                Model = model ?? this
            };
            var viewContext = new ViewContext(actionContext, viewResult.View, viewDictionary, new TempDataDictionary(actionContext.HttpContext, tempDataProvider), new StringWriter(), new HtmlHelperOptions());
            cardView.ViewContext = viewContext;
            cardView.RazorView = viewResult.View;
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

            if (CurrentCard != "Default")
            {
                outboundCard.Title = $"{this.Name} - {CurrentCard}";
                outboundCard.AdditionalProperties["url"] = $"/Cards/{this.Name}/{ResourceId}/{CurrentCard}";
            }
            else
            {
                outboundCard.Title = $"{this.Name}";
                outboundCard.AdditionalProperties["url"] = $"/Cards/{this.Name}/{ResourceId}";
            }

        }

        private async Task<SessionData> AddSessionDataToAdaptiveCardAsync(AdaptiveCard outboundCard, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(this.SessionId);
            ArgumentNullException.ThrowIfNull(this.ResourceId);

            var sessionData = new SessionData
            {
                App = this.CardType,
                ResourceId = this.ResourceId,
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
                if (action.Data == null)
                {
                    action.Data = new JObject();
                }

                // YAML authoring errors - "data:" without children properties is an empty string
                if (action.Data is string text && string.IsNullOrWhiteSpace(text))
                {
                    action.Data = new JObject();
                }

                var data = (JObject)action.Data;
                data[Constants.SESSIONDATA_KEY] = JToken.FromObject(sessionDataToken);
                data[Constants.IDDATA_KEY] = action.Id;
            }

            foreach (var choiceSet in outboundCard.GetElements<AdaptiveChoiceSetInput>())
            {
                if (choiceSet.DataQuery != null)
                {
                    choiceSet.DataQuery.Dataset = $"{sessionDataToken}{AdaptiveDataQuery.Seperator}{choiceSet.DataQuery.Dataset}";
                }
            }
            return sessionData;
        }

        private void AddRefresh(AdaptiveCard outboundCard)
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
                                SelectAction = new AdaptiveSelectAction()
                                {
                                    Action =new AdaptiveToggleVisibilityAction()
                                    {
                                        Title = "Close",
                                        TargetElements = new List<AdaptiveTargetElement>() { new AdaptiveTargetElement($"messageBanner{iMessage}") }
                                    }
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

            if (HasView("About"))
            {
                systemMenu.Actions.Add(new AdaptiveExecuteAction()
                {
                    Title = "About",
                    Verb = Constants.ABOUT_VERB,
                    IconUrl = "https://powercardbot.azurewebsites.net/about.png",
                    AssociatedInputs = AdaptiveAssociatedInputs.None
                    //   AdditionalProperties = new SerializableDictionary<string, object>() { { "mode", "secondary" } }
                });
            }

            if (outboundCard.Refresh != null)
            {
                systemMenu.Actions.Add(outboundCard.Refresh.Action);
            }

            if (this.HasView("Settings"))
            {
                systemMenu.Actions.Add(new AdaptiveExecuteAction()
                {
                    Title = "Settings",
                    Verb = Constants.SETTINGS_VERB,
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
    }
}
