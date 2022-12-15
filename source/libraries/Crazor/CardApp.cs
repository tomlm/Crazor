// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveCards;
using Crazor.Attributes;
using Crazor.Interfaces;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Diag = System.Diagnostics;

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
        private static XmlWriterSettings _settings = new XmlWriterSettings()
        {
            Encoding = new UnicodeEncoding(false, false), // no BOM in a .NET string
            Indent = true,
        };

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CardApp(CardAppContext context)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            Context = context;
            this.CallStack = new List<CardViewState>();
            if (this.GetType() != typeof(CardApp))
            {
                this.Name = this.GetType().Name;
                this.Name = Name.EndsWith("App") ? Name.Substring(0, Name.Length - 3) : Name;
            }
        }


        /// <summary>
        /// Name of the app
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// IconUrl to use for the card.
        /// </summary>
        public string IconUrl { get; set; }

        /// <summary>
        /// Route for the card.
        /// </summary>
        public CardRoute Route { get; set; }

        /// <summary>
        /// The activity we are processing.
        /// </summary>
        public Activity? Activity { get; set; }

        /// <summary>
        /// The action we are processing.
        /// </summary>
        public AdaptiveCardInvokeAction? Action { get; set; }

        /// <summary>
        /// Navigation stack
        /// </summary>
        [SessionMemory]
        public List<CardViewState> CallStack { get; set; }

        /// <summary>
        /// Current bound view 
        /// </summary>
        public ICardView CurrentView { get; private set; }

        /// <summary>
        /// Last Card result, set when a card calls CloseView(result)
        /// </summary>
        public CardResult? LastResult { get; set; }

        /// <summary>
        /// Messages to add to the card
        /// </summary>
        public List<BannerMessage> BannerMessages { get; private set; } = new List<BannerMessage>();

        /// <summary>
        /// Map of ids => elements for stylesheet mixins
        /// </summary>
        public Dictionary<string, AdaptiveElement>? Stylesheet { get; set; }

        /// <summary>
        /// Flag to enable/disable automatically assigning a sharedId if there is not one. 
        /// Default: true
        /// </summary>
        public bool AutoSharedId { get; set; } = true;

        /// <summary>
        /// IsTaskModule is true when the card is running in a taskModule mode.
        /// </summary>
        public bool IsTaskModule { get; set; } = false;

        /// <summary>
        /// IsTabModule is true when the card is running in a teams Tab.
        /// </summary>
        public bool IsTabModule => this.Activity.ChannelId == Channels.Msteams && this.Activity.Conversation.Id.StartsWith("tab:");

        /// <summary>
        /// TaskModuleAction is used to control signal that the taskmodule should be closed and the action to take with the card.
        /// </summary>
        public TaskModuleAction TaskModuleAction { get; set; }

        /// <summary>
        /// TeamsConversationMembers - UserIds needed for Refresh.UserIds on teams.
        /// </summary>
        /// <remarks>
        /// This is cached at the conversation level
        /// </remarks>
        [ConversationMemory]
        public List<string> TeamsConversationMembers { get; set; }

        /// <summary>
        /// ConnectorClient for calling outbound.
        /// </summary>
        public IConnectorClient ConnectorClient { get; set; }
        public CardAppContext Context { get; }

        public static IEnumerable<TypeInfo> GetCardAppTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => asm.DefinedTypes
                                                            .Where(t => t.IsAssignableTo(typeof(CardApp)) && t.IsAbstract == false));
        }

        /// <summary>
        /// process the activity
        /// </summary>
        /// <param name="invokeActivity"></param>
        /// <param name="isPreview"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<AdaptiveCard> ProcessInvokeActivity(IInvokeActivity invokeActivity, bool isPreview, CancellationToken cancellationToken)
        {
            await LoadAppAsync((Activity)invokeActivity!, cancellationToken);

            await OnActionExecuteAsync(cancellationToken);

            var card = await RenderCardAsync(isPreview, cancellationToken);

            await SaveAppAsync(cancellationToken);

            return card;
        }

        /// <summary>
        /// Handle action
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual async Task OnActionExecuteAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(this.Action);
            ArgumentNullException.ThrowIfNull(CurrentView);
            if (this.Action.Verb == null)
            {
                this.Action.Verb = Constants.SHOWVIEW_VERB;
            }

            if (this.Route.SessionId == null)
            {
                this.Route.SessionId = Utils.GetNewId();
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
                    var card = (AdaptiveCard)AdaptiveCard.XmlSerializer!.Deserialize(XmlReader.Create(new StringReader(xml!)))!;
                    Stylesheet = card.Body.ToDictionary(el => $"{el.Type}.{el.Id}", StringComparer.OrdinalIgnoreCase);
                }
            }

            try
            {
                if (!String.IsNullOrEmpty(this.Action.Verb))
                {
                    int ShowViewAttempts = 0;
                    while (ShowViewAttempts++ < 5)
                    {
                        var currentView = this.CurrentView;
                        await this.CurrentView.OnActionAsync(this.Action, cancellationToken);
                        if (LastResult != null)
                        {
                            // because we are resuming we don't need to execute again unless
                            // the resumption causes it to happen.
                            currentView = this.CurrentView;
                            await this.CurrentView.OnResumeView(LastResult, cancellationToken);
                        }

                        if (this.CurrentView == currentView)
                            break;

                        this.Action.Verb = Constants.SHOWVIEW_VERB;
                        this.LastResult = null;
                    }

                    if (ShowViewAttempts >= 5)
                    {
                        System.Diagnostics.Debug.WriteLine("WARNING: 5 ShowViews() in one turn attempted. Loop stopped.");
                    }

                    this.CurrentView.SaveState(this.CallStack[0]);
                }
            }
            catch (Exception err)
            {
                // if we fail in verb we add message banner to template.
                AddBannerMessage(err.Message, AdaptiveContainerStyle.Attention);
            }

        }

        /// <summary>
        /// Resolve content url to the card app.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public virtual string GetContentUrl(string path)
        {
            var uri = Context.Configuration.GetValue<Uri>("HostUri");
            return new Uri(uri, path.TrimStart('~')).AbsoluteUri;
        }

        /// <summary>
        /// Render the current view's card
        /// </summary>
        /// <param name="isPreview">if true the card should be a preview anonymous card for sharing</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<AdaptiveCard> RenderCardAsync(bool isPreview, CancellationToken cancellationToken)
        {
            AdaptiveCard? outboundCard;
            try
            {
                outboundCard = await CurrentView.RenderCardAsync(isPreview, cancellationToken);
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

            await ApplyCardModificationsAsync(outboundCard, isPreview, cancellationToken);

            return outboundCard;
        }

        /// <summary>
        /// Handle search for dynamic choices query
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual async Task<AdaptiveCardInvokeResponse> OnSearchChoicesAsync(SearchInvoke searchInvoke, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(CurrentView);
            Diag.Trace.WriteLine($"------- OnSearch({searchInvoke.Dataset}, {searchInvoke.QueryText})-----");

            var choices = await CurrentView.OnSearchChoices(searchInvoke, cancellationToken);

            return new AdaptiveCardInvokeResponse()
            {
                StatusCode = 200,
                Type = "application/vnd.microsoft.search.searchResponse",
                Value = JObject.FromObject(new { results = choices })
            };
        }

        /// <summary>
        /// Called to get searchresults 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task<SearchResult[]> OnSearchQueryAsync(MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult(Array.Empty<SearchResult>());
        }

        /// <summary>
        /// Navigate to card by name passing optional model
        /// </summary>
        /// <param name="route"></param>
        /// <param name="model"></param>
        public void ShowView(string route, object? model = null)
        {
            if (this.CallStack.Any())
            {
                this.CurrentView.SaveState(this.CallStack[0]);
            }

            if (!route.StartsWith('/'))
            {
                route = $"/Cards/{this.Name}/{route}".TrimEnd('/');
            }

            if (Context.RouteResolver.IsRouteValid(CardRoute.Parse(route)))
            {
                var cardViewState = new CardViewState(route, model);
                CallStack.Insert(0, cardViewState);
                Action!.Verb = Constants.SHOWVIEW_VERB;
                SetCurrentView(cardViewState);
            }
            else
            {
                throw new Exception($"{route} not found");
            }
        }

        /// <summary>
        /// Replace current card with a different card by name passing optional model
        /// </summary>
        /// <param name="route">card to switch to</param>
        /// <param name="model">model to pass card</param>
        public void ReplaceView(string route, object? model = null)
        {
            if (!route.StartsWith('/'))
            {
                route = $"/Cards/{this.Name}/{route}".TrimEnd('/');
            }

            if (Context.RouteResolver.IsRouteValid(CardRoute.Parse(route)))
            {
                var cardState = new CardViewState(route, model);
                this.CallStack[0] = cardState;
                Action!.Verb = Constants.SHOWVIEW_VERB;
                SetCurrentView(cardState);
            }
            else
            {
                throw new Exception($"{route} not found");
            }
        }

        public void CloseView(CardResult? result = null)
        {
            this.LastResult = result;

            if (this.CallStack.Any())
            {
                this.CallStack.RemoveAt(0);
            }

            if (!this.CallStack.Any())
            {
                CardRoute route = CardRoute.Parse($"/Cards/{this.Name}");
                if (Context.RouteResolver.IsRouteValid(route))
                    this.CallStack.Insert(0, new CardViewState(route.Route));
                else
                    throw new Exception("No default route!");
            }

            Action!.Verb = Constants.SHOWVIEW_VERB;
            SetCurrentView(this.CallStack[0]);
        }

        public void CloseTaskModule(TaskModuleAction status)
        {
            this.TaskModuleAction = status;
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

        public Uri GetCurrentCardUri()
        {
            return new Uri(Context.Configuration.GetValue<Uri>("HostUri"), this.GetCurrentCardRoute());
        }

        public Uri GetCardUriForRoute(string route)
        {
            return new Uri(Context.Configuration.GetValue<Uri>("HostUri"), route);
        }

        public virtual string GetCurrentCardRoute()
        {
            UriBuilder uri = new UriBuilder();

            var viewRoute = this.CurrentView!.GetRoute();
            var parts = viewRoute.Split('?');
            var subPath = parts[0];
            var query = parts.Skip(1).SingleOrDefault() ?? String.Empty;
            if (!viewRoute.StartsWith('/'))
            {
                if (viewRoute.Length > 0)
                    uri.Path = $"/Cards/{this.Name}/{subPath}";
                else if (viewRoute.Length == 0)
                    uri.Path = $"/Cards/{this.Name}";
                else
                    uri.Path = $"/Cards/{this.Name}/{this.CurrentView.Name}";
            }

            uri.Query = query;
            return uri.Uri.PathAndQuery;
        }

        /// <summary>
        /// Load state from storage
        /// </summary>
        /// <param name="storage"></param>
        /// <returns></returns>
        public async virtual Task LoadAppAsync(IInvokeActivity activity, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(cancellationToken);
            ArgumentNullException.ThrowIfNull(activity);

            if (!Context.RouteResolver.IsRouteValid(this.Route))
            {
                throw new Exception($"{this.Route} is not a valid route");
            }

            this.Activity = (Activity)activity;
            var invoke = JToken.FromObject(activity.Value ?? new JObject()).ToObject<AdaptiveCardInvokeValue>();
            ArgumentNullException.ThrowIfNull(invoke);
            this.Action = invoke.Action;

            //// map FromRoute attributes
            //foreach (var targetProperty in this.GetType().GetProperties().Where(prop => prop.GetCustomAttribute<FromRouteAttribute>() != null))
            //{
            //    var fromRouteName = targetProperty.GetCustomAttribute<FromRouteAttribute>().Name ?? targetProperty.Name;
            //    var dataProperty = Route.RouteData.Properties().Where(p => p.Name.ToLower() == fromRouteName.ToLower()).SingleOrDefault();
            //    if (dataProperty != null)
            //    {
            //        this.SetTargetProperty(targetProperty, dataProperty.Value);
            //    }
            //}

            // map App. routedata to app
            foreach (var routeProperty in Route.RouteData.Properties().Where(p => p.Name.StartsWith("App.")))
            {
                ObjectPath.SetPathValue(this, routeProperty.Name.Substring(4), routeProperty.Value.ToString(), false);
            }

            // lookup data
            var memoryMap = GetMemoryKeyMap();
            var state = await Context.Storage.ReadAsync(memoryMap.Keys.ToArray(), cancellationToken);

            // assign data to the properties.
            foreach (var key in memoryMap.Keys)
            {
                if (state.ContainsKey(key))
                {
                    var memoryAttribute = memoryMap[key];
                    SetScopedMemory(memoryAttribute.GetType(), (JObject)state[key]);
                }
            }

            if (Action?.Verb == Constants.LOADROUTE_VERB)
            {
                var cardState = new CardViewState(this.Route.Route);
                CallStack.Insert(0, cardState);
                SetCurrentView(cardState);
                return;
            }

            if (!this.CallStack.Any())
            {
                // initialize view for the route.
                var cardState = new CardViewState(this.Route.Route!);
                CallStack.Insert(0, cardState);
            }

            // load current view
            SetCurrentView(this.CallStack[0]);
        }

        /// <summary>
        /// Save state from storage
        /// </summary>
        /// <param name="storage"></param>
        /// <returns></returns>
        public async virtual Task SaveAppAsync(CancellationToken cancellationToken)
        {
            var data = new Dictionary<string, object>();
            // NOTE: we recompute the memoryKeyMap because we might have new memory scopes after executing action.
            var memoryMap = GetMemoryKeyMap();

            foreach (var key in memoryMap.Keys)
            {
                var memoryAttribute = memoryMap[key];
                data.Add(key, GetScopedMemory(memoryAttribute.GetType()));
            }

            await Context.Storage.WriteAsync(data, cancellationToken);
        }


        /// <summary>
        /// Given view name, return the IView object
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns>view</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public void SetCurrentView(CardViewState cardViewState)
        {
            this.CurrentView = LoadCardView(cardViewState);
        }

        public ICardView LoadCardView(CardViewState cardState)
        {
            ICardView cardView;
            try
            {
                Context.RouteResolver.ResolveRoute(CardRoute.Parse(cardState.Route), out var cardViewType);
                cardView = Context.CardViewFactory.Create(cardViewType);
                ArgumentNullException.ThrowIfNull(cardView);
            }
            catch (ArgumentException)
            {
                cardView = new EmptyCardView();
            }

            cardView.App = this;

            // rester card SessionMemory properties
            cardView.LoadState(cardState);
            return cardView;
        }

        public async Task<string> CreateCardTaskDeepLink(Uri uri, string title, string height, string width, CancellationToken cancellationToken)
        {
            var cardRoute = CardRoute.FromUri(uri);

            var cardApp = Context.CardAppFactory.Create(cardRoute, this.ConnectorClient);

            await cardApp.LoadAppAsync(Activity!, cancellationToken);

            var card = await cardApp.CurrentView.RenderCardAsync(isPreview: true, cancellationToken);

            var botId = Context.Configuration.GetValue<string>("MicrosoftAppId");
            var appId = Context.Configuration.GetValue<string>("TeamsAppId") ?? botId;

            return DeepLinks.CreateTaskModuleCardLink(appId, card!, title, height, width, botId);
        }

        private async Task ApplyCardModificationsAsync(AdaptiveCard outboundCard, bool isPreview, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(this.Activity);
            ArgumentNullException.ThrowIfNull(this.Action);

            await AddRefresh(outboundCard, isPreview, cancellationToken);

            AddMessageBanner(outboundCard, isPreview);

            AddSystemMenu(outboundCard, isPreview);

            // add session data to outbound card
            await AddMetadataToCard(outboundCard, isPreview, cancellationToken);

#pragma warning disable CS0618 // Type or member is obsolete
            outboundCard.Title = $"{this.Name}";
#pragma warning restore CS0618 // Type or member is obsolete

            outboundCard.Metadata = new AdaptiveMetadata() { WebUrl = GetCurrentCardUri().AbsoluteUri };
        }

        private async Task AddMetadataToCard(AdaptiveCard outboundCard, bool isPreview, CancellationToken cancellationToken)
        {
            var sessionId = (isPreview) ? null : this.Route.SessionId;
            string sessionIdEncrypt = null;
            if (!String.IsNullOrEmpty(sessionId))
            {
                sessionIdEncrypt = await Context.EncryptionProvider.EncryptAsync(sessionId, cancellationToken);
            }
            var route = GetCurrentCardRoute();

            foreach (var action in outboundCard.GetElements<AdaptiveExecuteAction>())
            {
                if ((action.Data == null) ||
                    (action.Data is string text && string.IsNullOrWhiteSpace(text)))
                {
                    action.Data = new JObject();
                }
                var data = (JObject)action.Data;
                data[Constants.ROUTE_KEY] = route;
                if (sessionId != null)
                    data[Constants.SESSION_KEY] = sessionIdEncrypt;
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

                var data = JObject.FromObject(action.Data);
                data[Constants.ROUTE_KEY] = route;
                if (sessionId != null)
                    data[Constants.SESSION_KEY] = sessionId;
                if (action.Id != null)
                    data[Constants.IDDATA_KEY] = action.Id;
            }

            foreach (var choiceSet in outboundCard.GetElements<AdaptiveChoiceSetInput>())
            {
                if (choiceSet.DataQuery != null)
                {
                    if (Activity!.ChannelId == Channels.Msteams && choiceSet.DataQuery.Dataset.StartsWith("graph.microsoft.com/users"))
                        continue;
                    else
                        choiceSet.DataQuery.Dataset = $"{Route.Route}{AdaptiveDataQuery.Separator}{sessionId}{AdaptiveDataQuery.Separator}{choiceSet.DataQuery.Dataset}";
                }
            }
        }

        private async Task AddRefresh(AdaptiveCard outboundCard, bool isPreview, CancellationToken cancellationToken)
        {
            var uri = GetCurrentCardUri();

            if (outboundCard.Refresh == null)
            {
                AdaptiveExecuteAction refresh;
                if (isPreview)
                {
                    refresh = new AdaptiveExecuteAction()
                    {
                        Title = "Refresh",
                        Verb = Constants.LOADROUTE_VERB,
                        IconUrl = new Uri(uri, "/images/refresh.png").AbsoluteUri,
                        AssociatedInputs = AdaptiveAssociatedInputs.None,
                        Data = new JObject()
                        {
                            // NO sesion for preview card...{ Constants.SESSION_KEY, this.RouteData.SessionId },
                            { Constants.ROUTE_KEY, uri.PathAndQuery},
                        }
                    };
                }
                else
                {
                    refresh = new AdaptiveExecuteAction()
                    {
                        Title = "Refresh",
                        Verb = Constants.SHOWVIEW_VERB,
                        IconUrl = new Uri(uri, "/images/refresh.png").AbsoluteUri,
                        AssociatedInputs = AdaptiveAssociatedInputs.None,
                        Data = new JObject()
                        {
                            { Constants.SESSION_KEY, this.Route.SessionId },
                            { Constants.ROUTE_KEY, uri.PathAndQuery },
                        }
                    };
                }
                outboundCard.Refresh = new AdaptiveRefresh()
                {
                    Action = refresh,
                };
            }

            if (this.Activity!.ChannelId == Channels.Msteams && !Activity.Conversation.Id.StartsWith("tab:"))
            {
                if (TeamsConversationMembers == null)
                {
                    var teamId = Activity.GetChannelData<TeamsChannelData>().Team?.Id ?? Activity.Conversation.Id;
                    try
                    {

                        // we need to add refresh userids
                        var teamsMembers = await ConnectorClient.Conversations.GetConversationPagedMembersAsync(teamId, 60, cancellationToken: cancellationToken);
                        this.TeamsConversationMembers = teamsMembers.Members.Select(member => $"8:orgid:{member.AadObjectId}").ToList();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError($"Failed to get UserIds for conversation.id={teamId}\n{ex.Message}");
                    }
                }
                outboundCard.Refresh.UserIds = this.TeamsConversationMembers;
            }
        }

        private void AddMessageBanner(AdaptiveCard outboundCard, bool isPreview)
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

                this.BannerMessages.Clear();
#endif
            }
        }


        private void AddSystemMenu(AdaptiveCard outboundCard, bool isPreview)
        {
            var systemMenu = new AdaptiveActionSet()
            {
                Id = "systemMenu",
                IsVisible = false,
                Separator = true
            };

            var currentUri = GetCurrentCardUri();

            if (Activity!.ChannelId != currentUri.Host && !IsTaskModule && !isPreview)
            {
                systemMenu.Actions.Add(new AdaptiveOpenUrlAction()
                {
                    Title = "Open",
                    IconUrl = new Uri(currentUri, Context.Configuration.GetValue<string>("OpenLinkIcon") ?? "/images/OpenLink.png").AbsoluteUri,
                    Url = currentUri.AbsoluteUri,
                    Mode = AdaptiveActionMode.Secondary
                });
            }

            if (Activity.ChannelId == currentUri.Host && outboundCard.Refresh?.Action != null)
            {
                outboundCard.Refresh.Action.Mode = AdaptiveActionMode.Secondary;
                systemMenu.Actions.Add(outboundCard.Refresh.Action);
            }

            if (Context.RouteResolver.ResolveRoute(CardRoute.Parse($"/Cards/{this.Name}/{Constants.ABOUT_VIEW}"), out var cardViewType) && this.Route.View != Constants.ABOUT_VIEW)
            {
                systemMenu.Actions.Add(new AdaptiveExecuteAction()
                {
                    Title = Constants.ABOUT_VIEW,
                    Verb = Constants.ABOUT_VIEW,
                    IconUrl = new Uri(currentUri, Context.Configuration.GetValue<string>("AboutIcon") ?? "/images/about.png").AbsoluteUri,
                    AssociatedInputs = AdaptiveAssociatedInputs.None,
                    Mode = AdaptiveActionMode.Secondary
                });
            }

            if (Context.RouteResolver.ResolveRoute(CardRoute.Parse($"/Cards/{this.Name}/{Constants.SETTINGS_VIEW}"), out cardViewType) && this.Route.View != Constants.SETTINGS_VIEW)
            {
                systemMenu.Actions.Add(new AdaptiveExecuteAction()
                {
                    Title = Constants.SETTINGS_VIEW,
                    Verb = Constants.SETTINGS_VIEW,
                    IconUrl = new Uri(currentUri, Context.Configuration.GetValue<string>("SettingsIcon") ?? "/images/settings.png").AbsoluteUri,
                    AssociatedInputs = AdaptiveAssociatedInputs.None,
                    Mode = AdaptiveActionMode.Secondary
                });
            }

            if (this.Activity.ChannelId == currentUri.Host)
            {
                // INSERT FAKE SYSTEM MENU 
                if (outboundCard.Body.Any())
                {
                    outboundCard.Body.First().Separator = false;
                }

                if (outboundCard.Actions != null)
                {
                    systemMenu.Actions.AddRange(outboundCard.Actions.Where(a => a.Mode == AdaptiveActionMode.Secondary));
                    outboundCard.Actions = outboundCard.Actions.Where(a => a.Mode == AdaptiveActionMode.Primary).ToList();
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
                                new AdaptiveImage(new Uri(currentUri, Context.Configuration.GetValue<string>("BotIcon") ?? "/images/boticon.png").AbsoluteUri)
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
                                new AdaptiveTextBlock(Context.Configuration.GetValue<string>("BotName")) { Size= AdaptiveTextSize.Small }
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

        private object GetScopedMemory(Type memoryAttributeType)
        {
            lock (this)
            {
                dynamic memory = new JObject();
                foreach (var property in this.GetType().GetProperties().Where(prop => prop.GetCustomAttribute(memoryAttributeType, true) != null))
                {
                    var val = property.GetValue(this);
                    memory[property.Name] = val != null ? JToken.FromObject(val) : null;
                }

                return memory;
            }
        }

        private void SetScopedMemory(Type memoryAttributeType, JObject memory)
        {
            lock (this)
            {
                foreach (var property in this.GetType().GetProperties().Where(prop => prop.GetCustomAttribute(memoryAttributeType, true) != null))
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



        protected string? GetKey(string name, string? key) => String.IsNullOrEmpty(key) ? null : $"{this.Name}-{name}-{key}";

        private Dictionary<string, MemoryAttribute> GetMemoryKeyMap()
        {
            var attributeMap = new Dictionary<string, MemoryAttribute>();

            var memoryAttributes = GetType().GetProperties()
                                                .SelectMany(p => p.GetCustomAttributes().Where(a => a.GetType().IsAssignableTo(typeof(MemoryAttribute))))
                                                .Cast<MemoryAttribute>()
                                                .ToList();
            // add keys to lookup
            foreach (var memoryAttribute in memoryAttributes)
            {
                var key = GetKey(memoryAttribute.Name, memoryAttribute.GetKey(this));
                if (key != null && !attributeMap.ContainsKey(key))
                {
                    attributeMap.Add(key, memoryAttribute);
                }
            }
            return attributeMap;
        }
    }
}
