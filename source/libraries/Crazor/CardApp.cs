// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveCards;
using Crazor.Attributes;
using Crazor.Exceptions;
using Crazor.Interfaces;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
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
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.DefinedTypes)
                {
                    if (type.IsAssignableTo(typeof(CardApp)) && type.IsAbstract == false)
                    {
                        yield return type;
                    }
                }
            }
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

            // Tracing
            if (Context.ServiceOptions != null && this.Activity != null)
            {
                Context.ServiceOptions.Logger.Request?.Invoke(this.Activity);
            }

            Diag.Trace.WriteLine($"------- OnAction({this.Action.Verb}) {this.Route.Route}-----");

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
                        var currentRoute = this.Route;
                        await this.CurrentView.OnActionAsync(this.Action, cancellationToken);
                        if (LastResult != null)
                        {
                            // because we are resuming we don't need to execute again unless
                            // the resumption causes it to happen.
                            await this.CurrentView.OnResumeView(LastResult, cancellationToken);
                        }

                        if (this.Route == currentRoute)
                            break;

                        this.LastResult = null;
                    }

                    if (ShowViewAttempts >= 5)
                    {
                        System.Diagnostics.Debug.WriteLine("WARNING: 5 ShowViews() in one turn attempted. Loop stopped.");
                    }

                    SaveCardState();
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
            if (path.StartsWith("~") || path.StartsWith("/"))
            {
                var uri = Context.Configuration.GetValue<Uri>("HostUri");
                return new Uri(uri, path.TrimStart('~')).AbsoluteUri;
            }
            return new Uri(path).AbsoluteUri;
        }

        /// <summary>
        /// Render the current view's card
        /// </summary>
        /// <param name="isPreview">if true the card should be a preview anonymous card for sharing</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<AdaptiveCard> RenderCardAsync(bool isPreview, CancellationToken cancellationToken)
        {
            Diag.Stopwatch sw = new Diag.Stopwatch();
            sw.Start();

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
            sw.Stop();

            var host = this.Context.Configuration.GetValue<Uri>("HostUri").Host.ToLower();
            if (host == "localhost" || host.EndsWith("ngrok.io"))
            {
                outboundCard.Title = $"{this.Name} [{sw.Elapsed.ToString()}]";
            }

            await ApplyCardModificationsAsync(outboundCard, isPreview, cancellationToken);

            // Tracing
            if (Context.ServiceOptions != null && outboundCard != null && this.Activity != null)
            {
                Context.ServiceOptions.Logger.Response?.Invoke(this.Activity, outboundCard);
            }

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
                SaveCardState();
            }

            if (!route.StartsWith('/'))
            {
                route = $"/Cards/{this.Name}/{route}".TrimEnd('/');
            }

            var newRoute = CardRoute.Parse(route);
            if (Context.RouteResolver.IsRouteValid(newRoute))
            {
                var cardViewState = new CardViewState(newRoute.Route, model);
                CallStack.Insert(0, cardViewState);
                Action!.Verb = Constants.LOADROUTE_VERB;
                Action!.Data = new JObject() { { Constants.ROUTE_KEY, this.CallStack[0].Route } };
                SetCurrentView(cardViewState);
            }
            else
            {
                throw new Exception($"{route} not found");
            }
        }

        private void LoadCardState(CardViewState cardState)
        {
            foreach (var property in CurrentView.GetPersistentProperties())
            {
                if (cardState.SessionMemory.TryGetValue(property.Name, out var val))
                {
                    CurrentView.SetTargetProperty(property, val);
                }
            }

            CurrentView.SetModel(cardState.Model);

            if (cardState.Initialized == false)
            {
                // call hook to give cardview opportunity to process data.
                CurrentView.OnInitialized();
                cardState.Initialized = true;
            }
        }

        private void SaveCardState()
        {
            this.CallStack[0].Model = CurrentView.GetModel();

            // capture all properties on CardView which are not on base type and not ignored.
            foreach (var property in this.CurrentView.GetPersistentProperties())
            {
                var val = property.GetValue(this.CurrentView);
                if (val != null)
                {
                    this.CallStack[0].SessionMemory[property.Name] = JToken.FromObject(val);
                }
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
                Action!.Verb = Constants.LOADROUTE_VERB;
                Action!.Data = new JObject() { { Constants.ROUTE_KEY, this.CallStack[0].Route } };
                SetCurrentView(cardState);
            }
            else
            {
                throw new Exception($"{route} not found");
            }
        }

        public void CloseView(CardResult result)
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

            Action!.Verb = Constants.LOADROUTE_VERB;
            Action!.Data = new JObject() { { Constants.ROUTE_KEY, this.CallStack[0].Route } };
            SetCurrentView(this.CallStack[0]);
        }

        /// <summary>
        /// Close the current card, optionalling returning the result
        /// </summary>
        /// <param name="result">the result to return to the current caller</param>
        public void CloseView(object? result = null)
        {
            this.CloseView(new CardResult()
            {
                Name = this.Name,
                Result = result,
                Success = true
            });
        }

        /// <summary>
        /// Cancel the current card, returning a message
        /// </summary>
        /// <param name="message">optional message to return.</param>
        public void CancelView(string? message = null)
        {
            this.CloseView(new CardResult()
            {
                Name = this.Name,
                Message = message,
                Success = false
            });
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
            else
            {
                uri.Path = subPath;
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

            // map Route attributes for app
            foreach (var targetProperty in this.GetType().GetProperties().Where(prop => prop.GetCustomAttribute<FromCardRouteAttribute>() != null))
            {
                var fromRouteName = targetProperty.GetCustomAttribute<FromCardRouteAttribute>()?.Name ?? targetProperty.Name;
                if (fromRouteName != null)
                {
                    var dataProperty = Route.RouteData.Properties().Where(p => p.Name.ToLower() == fromRouteName.ToLower()).SingleOrDefault();
                    if (dataProperty != null)
                    {
                        this.SetTargetProperty(targetProperty, dataProperty.Value);
                    }
                }
            }

            // map query parameters for app.
            foreach (var targetProperty in this.GetType().GetProperties().Where(prop => prop.GetCustomAttribute<FromCardQueryAttribute>() != null))
            {
                var fromQueryName = targetProperty.GetCustomAttribute<FromCardQueryAttribute>()?.Name ??
                    targetProperty.Name;
                if (fromQueryName != null)
                {
                    var dataProperty = Route.RouteData.Properties().Where(p => p.Name.ToLower() == fromQueryName.ToLower()).SingleOrDefault();
                    if (dataProperty != null)
                    {
                        this.SetTargetProperty(targetProperty, dataProperty.Value);
                    }
                }
            }

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
                var newRoute = ((JObject)Action.Data)[Constants.ROUTE_KEY].ToString();

                // call stack ALWAYS has default page at root
                while (CallStack.Count > 1)
                {
                    if (CallStack[0].Route.ToLower() != newRoute.ToLower())
                    {
                        CancelView();
                    }
                    else
                    {
                        // found;
                        SetCurrentView(this.CallStack[0]);
                        return;
                    }
                }

                var cardState = new CardViewState(newRoute);
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
            var cardRoute = CardRoute.Parse(cardViewState.Route);
            Context.RouteResolver.ResolveRoute(cardRoute, out var cardViewType);

            this.CurrentView = (ICardView)this.Context.ServiceProvider.GetService(cardViewType);
            ArgumentNullException.ThrowIfNull(this.CurrentView, $"View {cardViewState.Route} not found");

            cardRoute.SessionId = this.Route.SessionId;
            this.Route = cardRoute;
            this.CurrentView.App = this;
            // restore card SessionMemory properties for CardView
            LoadCardState(cardViewState);
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

        /// <summary>
        /// This is a utility function for CardViews to use reflection to handle action verbs
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task OnActionReflectionAsync(AdaptiveCardInvokeAction action, CancellationToken cancellationToken)
        {
            // Process BindProperty tags
            var data = (JObject)JObject.FromObject(action.Data).DeepClone();
            BindProperties(data);

            MethodInfo? verbMethod = null;
            if (action.Verb == Constants.LOADROUTE_VERB)
            {
                // merge in route and query data since we are in a LOAD ROUTE situation.
                if (this.Route.RouteData != null)
                    data.Merge(this.Route.RouteData);

                if (this.Route.QueryData != null)
                    data.Merge(this.Route.QueryData);

                // process [FromCardRoute] attributes. This allows [FromCardRoute] to be placed on a property which doesn't match the RouteData.property name
                foreach (var targetProperty in this.CurrentView.GetType().GetProperties().Where(prop => prop.GetCustomAttribute<FromCardRouteAttribute>() != null))
                {
                    var fromRouteName = targetProperty.GetCustomAttribute<FromCardRouteAttribute>().Name ?? targetProperty.Name;
                    var dataProperty = this.Route.RouteData.Properties().Where(p => p.Name.ToLower() == fromRouteName.ToLower()).SingleOrDefault();
                    if (dataProperty != null)
                    {
                        this.CurrentView.SetTargetProperty(targetProperty, dataProperty.Value);
                    }
                }

                // Process any Route properties as setters onto current view.
                foreach (var routeProperty in this.Route.RouteData.Properties().Where(p => !p.Name.StartsWith("App.")))
                {
                    ObjectPath.SetPathValue(this.CurrentView, routeProperty.Name, routeProperty.Value.ToString(), false);
                }

                // process query  attributes as setters onto current view
                foreach (var targetProperty in this.CurrentView.GetType().GetProperties().Where(p => p.GetCustomAttribute<FromCardQueryAttribute>() != null))
                {
                    var fromQueryName = targetProperty.GetCustomAttribute<FromCardQueryAttribute>()?.Name ??
                        targetProperty.Name;
                    if (fromQueryName != null)
                    {
                        var dataProperty = Route.QueryData.Properties().Where(p => p.Name.ToLower() == fromQueryName.ToLower()).SingleOrDefault();
                        if (dataProperty != null)
                        {
                            this.CurrentView.SetTargetProperty(targetProperty, dataProperty.Value);
                        }
                    }
                }

                // LoadRoute verb should invoke this method FIRST before validation, as this method should load the model.
                verbMethod = this.CurrentView.GetMethod(action.Verb);
                if (verbMethod != null)
                {
                    try
                    {
                        await this.CurrentView.InvokeMethodAsync(verbMethod, this.CurrentView.GetMethodArgs(verbMethod, data, cancellationToken));
                    }
                    catch (CardRouteNotFoundException notFound)
                    {
                        this.AddBannerMessage(notFound.Message, AdaptiveContainerStyle.Attention);
                        this.CancelView();
                    }
                    catch (Exception err)
                    {
                        if (err.InnerException is CardRouteNotFoundException notFound)
                        {
                            this.CancelView(notFound.Message);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                action.Verb = Constants.SHOWVIEW_VERB;
            }

            if (action.Verb != Constants.SHOWVIEW_VERB)
            {
                // otherwise, validate Model first so verb can check Model.IsValid property to decide what to do.
                this.CurrentView.Validate();
            }

            switch (action.Verb)
            {
                case Constants.CANCEL_VERB:
                    // if there is an OnCancel, call it
                    if (await this.CurrentView.InvokeVerbAsync(action, cancellationToken) == false)
                    {
                        // default implementation 
                        this.CancelView();
                    }
                    break;

                case Constants.OK_VERB:
                    if (await this.CurrentView.InvokeVerbAsync(action, cancellationToken) == false)
                    {
                        if (this.CurrentView.IsModelValid)
                        {
                            this.CloseView(this.CurrentView.GetModel());
                        }
                    }
                    break;

                default:
                    await this.CurrentView.InvokeVerbAsync(action, cancellationToken);
                    break;
            }

        }

        private void BindProperties(object obj)
        {
            JObject data = JObject.FromObject(obj);
            if (data != null)
            {
                foreach (var property in data.Properties())
                {
                    var parts = property.Name.Split('.');

                    // if root is [BindProperty]
                    var prop = this.CurrentView.GetType().GetProperty(parts[0]);
                    // only allow binding to Model, App or BindProperty
                    if (prop != null && this.CurrentView.GetBindableProperties().Contains(prop))
                    {
                        ObjectPath.SetPathValue(this.CurrentView, property.Name, property.Value, json: false);
                    }
                }
            }
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
                if (action.Data == null)
                {
                    action.Data = new JObject();
                }
                else if (action.Data is string text)
                {
                    text = text.Trim();
                    if (String.IsNullOrEmpty(text))
                    {
                        action.Data = new JObject();
                    }
                    else
                    {
                        action.Data = JObject.Parse(text);
                    }
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
                if (action.Data == null)
                {
                    action.Data = new JObject();
                }
                else if (action.Data is string text)
                {
                    text = text.Trim();
                    if (String.IsNullOrEmpty(text))
                    {
                        action.Data = new JObject();
                    }
                    else
                    {
                        action.Data = JObject.Parse(text);
                    }
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
                                new AdaptiveTextBlock($"{Context.Configuration.GetValue<string>("BotName")} - {outboundCard.Title}") { Size= AdaptiveTextSize.Small }
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
