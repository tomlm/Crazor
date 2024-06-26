using Crazor.Attributes;
using Crazor.Exceptions;
using Crazor.Interfaces;
using Crazor.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Xml;
using YamlConverter;
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
            context.App = this;

            this.Memory = new Memory(Context.Storage, this.Name!);
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
        public Activity Activity { get; set; }

        /// <summary>
        /// The action we are processing.
        /// </summary>
        public AdaptiveCardInvokeAction Action { get; set; }

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
        /// IsTaskModule is true when the card is running in a taskModule mode.
        /// </summary>
        public bool IsTaskModule { get; set; } = false;

        /// <summary>
        /// CommandContext is "compose", "commandBox", "message" 
        /// </summary>
        public string CommandContext { get; set; } = "message";

        /// <summary>
        /// Is the current context running in the web page
        /// </summary>
        public bool IsWebHost => this.Activity.ChannelId == Context.Configuration.GetValue<Uri>("HostUri")?.Host;

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
        /// CardAppContext.
        /// </summary>
        public CardAppContext Context { get; }

        /// <summary>
        /// Memory system for the application. This allows you to easily stored memory scoped objects.
        /// </summary>
        public Memory Memory { get; set; }

        /// <summary>
        /// process the activity
        /// </summary>
        /// <param name="invokeActivity"></param>
        /// <param name="isPreview"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<AdaptiveCard> ProcessInvokeActivity(IInvokeActivity invokeActivity, bool isPreview, CancellationToken cancellationToken)
        {
            await AuthenticateUserAsync(cancellationToken);

            await AuthorizeActivityAsync(invokeActivity, isPreview, cancellationToken);

            await OnActionExecuteAsync(cancellationToken);

            if (isPreview == false)
            {
                // force preview mode if the taskmodule action is set.
                isPreview = TaskModuleAction == TaskModuleAction.Auto ||
                    TaskModuleAction == TaskModuleAction.PostCard ||
                    TaskModuleAction == TaskModuleAction.InsertCard;
            }

            var card = await RenderCardAsync(isPreview, cancellationToken);

            card.Authentication = (await GetAdaptiveAuthentication(cancellationToken))!;

            await SaveAppAsync(cancellationToken);

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debug.WriteLine("===outbound card===");
                System.Diagnostics.Debug.WriteLine(card.ToJson());
            }
#endif
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
                this.Action.Verb = Constants.REFRESH_VERB;
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

            try
            {
                if (!String.IsNullOrEmpty(this.Action.Verb))
                {
                    int ShowViewAttempts = 0;
                    while (ShowViewAttempts++ < 5)
                    {
                        var currentRoute = this.Route;

                        if (this.CallStack[0].Initialized == false)
                        {
                            // call hook to give cardview opportunity to process data.
                            await CurrentView.OnInitializedAsync(cancellationToken);
                            this.CallStack[0].Initialized = true;
                        }

                        await this.CurrentView.OnActionAsync(this.Action, cancellationToken);
                        if (LastResult != null)
                        {
                            // because we are resuming we don't need to execute again unless
                            // the resumption causes it to happen.
                            await this.CurrentView.OnResumeViewAsync(LastResult, cancellationToken);
                        }

                        if (this.Route == currentRoute)
                            break;

                        // if we have a new route, apply the route attributes again.
                        this.ApplyRouteAttributes(Route);

                        this.LastResult = null;
                    }

                    if (ShowViewAttempts >= 5)
                    {
                        System.Diagnostics.Debug.WriteLine("WARNING: 5 ShowViews() in one turn attempted. Loop stopped.");
                    }

                    SaveCardState();
                }
            }
            catch (AdaptiveAuthenticationRequiredException)
            {
                SaveCardState();
                throw;
            }
            catch (Exception err)
            {
                // hmmm...seems like we need a external function to call for this...
                var current = err;
                do
                {
                    if (current.GetType().Namespace == "Microsoft.Identity.Web")
                        throw current;
                    current = current.InnerException;
                } while (current != null);

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
                var uri = Context.Configuration.GetValue<Uri>("HostUri")!;
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
                if (this.Action.Verb == Constants.DEBUGGER_VERB)
                {
                    outboundCard = new AdaptiveCard("1.5")
                    {
                        Body =
                        {
                            new AdaptiveTextBlock() { Text = "Activity", Style=AdaptiveTextBlockStyle.Heading, Weight=AdaptiveTextWeight.Bolder },
                            new AdaptiveRichTextBlock()
                            {
                                Separator = true,
                                Inlines = YamlConvert.SerializeObject(this.Activity).Split('\n')
                                            .Select(line => new AdaptiveTextRun($"{line}\n".Replace(' ', ' ')) { FontType=AdaptiveFontType.Monospace })
                                            .Cast<AdaptiveInline>()
                                            .ToList()
                            }
                        },
                        Actions = new List<AdaptiveAction>()
                        {
                            new AdaptiveExecuteAction() { Verb = Constants.REFRESH_VERB, Data = this.Action.Data, Title="Refresh" }
                        }
                    };
                }
                else
                {
                    outboundCard = await CurrentView.RenderCardAsync(isPreview, cancellationToken);
                }
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

#pragma warning disable CS0618 // Type or member is obsolete
            outboundCard.Title = outboundCard.Title ?? this.Name;
#pragma warning restore CS0618 // Type or member is obsolete
#if instrument
            var host = this.Context.Configuration.GetValue<Uri>("HostUri").Host.ToLower();
            if (host == "localhost" || host.EndsWith("ngrok.io") || host.EndsWith(".devtunnels.ms"))
            {
                outboundCard.Title = $"{this.Name} [{sw.Elapsed.ToString()}]";
            }
#endif
            outboundCard = await ApplyCardModificationsAsync(outboundCard, isPreview, cancellationToken);

            // Tracing
            if (Context.ServiceOptions != null && outboundCard != null && this.Activity != null)
            {
                Context.ServiceOptions.Logger.Response?.Invoke(this.Activity, outboundCard);
            }

            return outboundCard!;
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

            var choices = await CurrentView.OnSearchChoicesAsync(searchInvoke, cancellationToken);

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
                Action!.Verb = Constants.LOAD_VERB;
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
            CurrentView.ApplyRouteAttributes(this.Route);

            foreach (var property in CurrentView.GetPersistentProperties())
            {
                if (cardState.SessionMemory.TryGetValue(property.Name, out var val))
                {
                    CurrentView.SetTargetProperty(property, val);
                }
            }

            CurrentView.SetModel(cardState.Model!);
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
                Action!.Verb = Constants.LOAD_VERB;
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

            Action!.Verb = Constants.LOAD_VERB;
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
            return new Uri(Context.Configuration.GetValue<Uri>("HostUri")!, this.GetCurrentCardRoute());
        }

        public Uri GetCardUriForRoute(string route)
        {
            return new Uri(Context.Configuration.GetValue<Uri>("HostUri")!, route);
        }

        public virtual string GetCurrentCardRoute()
        {
            UriBuilder uri = new UriBuilder();

            var viewRoute = this.CurrentView!.GetRoute();
            var i = viewRoute.IndexOf('?');
            string subPath = viewRoute;
            if (i >= 0)
            {
                uri.Query = viewRoute.Substring(i + 1);
                subPath = viewRoute.Substring(0, i);
            }

            if (!subPath.StartsWith('/'))
            {
                if (subPath.Length > 0)
                    uri.Path = $"/Cards/{this.Name}/{subPath}";
                else if (subPath.Length == 0)
                    uri.Path = $"/Cards/{this.Name}";
                else
                    uri.Path = $"/Cards/{this.Name}/{this.CurrentView.Name}";
            }
            else
            {
                uri.Path = subPath.TrimEnd('/');
            }

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
            var invoke = activity.Value as AdaptiveCardInvokeValue ?? JToken.FromObject(activity.Value ?? new JObject()).ToObject<AdaptiveCardInvokeValue>();
            ArgumentNullException.ThrowIfNull(invoke);
            this.Action = invoke.Action;

            this.ApplyRouteAttributes(Route);

            // lookup data
            var memoryMap = GetMemoryKeyMap();

            // NOTE: We use memoryMap and raw storage because we are doing bulk lookup of all of the scoped memories. 
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

            if (Action?.Verb == Constants.LOAD_VERB)
            {
                var newRoute = ((JObject)Action.Data)[Constants.ROUTE_KEY]!.ToString();

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
            ArgumentNullException.ThrowIfNull(cardViewType);
            this.CurrentView = CreateCardView(cardViewType);
            ArgumentNullException.ThrowIfNull(this.CurrentView, $"View {cardViewState.Route} not found");

            cardRoute.SessionId = this.Route.SessionId;
            this.Route = cardRoute;
            // restore card SessionMemory properties for CardView
            LoadCardState(cardViewState);
        }

        private ICardView CreateCardView(Type cardViewType)
        {
            var cardView = (ICardView)Context.ServiceProvider.GetRequiredService(cardViewType);
            cardView.App = this;

            // inject dependencies
            var props = cardViewType
                             .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                             .Where(p => p.CanWrite && Attribute.IsDefined(p, typeof(InjectAttribute)));

            foreach (var prop in props)
            {
                prop.SetValue(cardView, Context.ServiceProvider.GetService(prop.PropertyType), null);
            }

            return cardView;
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
            MergeRouteData(data);
            BindProperties(data);
            action = new AdaptiveCardInvokeAction()
            {
                Verb = action.Verb,
                Id = action.Id,
                Type = action.Type,
                Data = data
            };

            if (action.Verb != Constants.REFRESH_VERB &&
                action.Verb != Constants.LOAD_VERB)
            {
                // otherwise, validate Model first so verb can check Model.IsValid property to decide what to do.
                await this.CurrentView.OnValidateModelAsync(cancellationToken);
            }

            switch (action.Verb)
            {
                case Constants.CLOSEVIEW_VERB:
                    // close verb closes the window without returning any data.
                    // we call invoke verb so it can be overriden
                    if (await this.CurrentView.InvokeVerbAsync(action, cancellationToken) == false)
                    {
                        this.CloseView();
                    }
                    break;
                case Constants.CANCEL_VERB:
                    // cancel verb cancels the window
                    // we call invoke verb so it can be overriden
                    if (await this.CurrentView.InvokeVerbAsync(action, cancellationToken) == false)
                    {
                        // default implementation 
                        if (ObjectPath.TryGetPathValue<string?>(action.Data, "Message", out var message))
                        {
                            this.CancelView(message);
                        }
                        else
                        {
                            this.CancelView();
                        }
                    }
                    break;

                case Constants.OK_VERB:
                    // ok verb closes the window if the model is valid.
                    // we call invoke verb so it can be overriden
                    if (await this.CurrentView.InvokeVerbAsync(action, cancellationToken) == false)
                    {
                        if (this.CurrentView.IsModelValid)
                        {
                            this.CloseView(this.CurrentView.GetModel());
                        }
                    }
                    break;

                case Constants.LOGIN_VERB:
                    // we call invoke verb so it can be overriden
                    if (await this.CurrentView.InvokeVerbAsync(action, cancellationToken) == false)
                    {
                        await LoginUserAsync(cancellationToken);
                    }
                    break;

                case Constants.LOGOUT_VERB:
                    // we call invoke verb so it can be overriden
                    if (await this.CurrentView.InvokeVerbAsync(action, cancellationToken) == false)
                    {
                        await this.LogoutUserAsync(cancellationToken);
                    }
                    break;

                case Constants.SHOWVIEW_VERB:
                    // we call invoke verb so it can be overriden
                    if (await this.CurrentView.InvokeVerbAsync(action, cancellationToken) == false)
                    {
                        if (ObjectPath.TryGetPathValue<string>(action.Data, "Route", out var route))
                            this.ShowView(route, ObjectPath.GetPathValue<object?>(action.Data, "Model", null));
                        else
                            AddBannerMessage($"Route not defined on OnShowView()");
                    }
                    break;
                case Constants.REPLACEVIEW_VERB:
                    // we call invoke verb so it can be overriden
                    if (await this.CurrentView.InvokeVerbAsync(action, cancellationToken) == false)
                    {
                        if (ObjectPath.TryGetPathValue<string>(action.Data, "Route", out var route))
                            this.ReplaceView(route, ObjectPath.GetPathValue<object?>(action.Data, "Model", null));
                        else
                            AddBannerMessage($"Route not defined on OnReplaceView()");
                    }
                    break;
                default:
                    await this.CurrentView.InvokeVerbAsync(action, cancellationToken);
                    break;
            }
        }

        private void MergeRouteData(JObject data)
        {
            // merge in route and query data since we are in a LOAD ROUTE situation.
            if (this.Route.RouteData != null)
                data.Merge(this.Route.RouteData);

            if (this.Route.QueryData != null)
                data.Merge(this.Route.QueryData);

            // process [FromCardRoute] attributes. This allows [FromCardRoute] to be placed on a property which doesn't match the RouteData.property name
            foreach (var targetProperty in this.CurrentView.GetType().GetProperties().Where(prop => prop.GetCustomAttribute<FromCardRouteAttribute>() != null))
            {
                var fromRouteName = targetProperty.GetCustomAttribute<FromCardRouteAttribute>()?.Name ?? targetProperty.Name;
                var dataProperty = this.Route.RouteData?.Properties().Where(p => p.Name.ToLower() == fromRouteName.ToLower()).SingleOrDefault();
                if (dataProperty != null)
                {
                    this.CurrentView.SetTargetProperty(targetProperty, dataProperty.Value);
                }
            }

            // Process any Route properties as setters onto current view.
            foreach (var routeProperty in this.Route.RouteData!.Properties().Where(p => !p.Name.StartsWith("App.")))
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
                    var dataProperty = Route.QueryData!.Properties().Where(p => p.Name.ToLower() == fromQueryName.ToLower()).SingleOrDefault();
                    if (dataProperty != null)
                    {
                        this.CurrentView.SetTargetProperty(targetProperty, dataProperty.Value);
                    }
                }
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



        private async Task<AdaptiveCard> ApplyCardModificationsAsync(AdaptiveCard outboundCard, bool isPreview, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(this.Activity);
            ArgumentNullException.ThrowIfNull(this.Action);

            await AddRefresh(outboundCard, isPreview, cancellationToken);

            AddMessageBanner(outboundCard, isPreview);

            AddSystemMenu(outboundCard, isPreview);

            // add session data to outbound card
            await AddMetadataToCard(outboundCard, isPreview, cancellationToken);

            var channelOptions = this.Context.ServiceOptions.GetChannelOptions(this.Activity.ChannelId);
            if (channelOptions.SchemaVersion.Major == 1 && channelOptions.SchemaVersion.Minor < 4)
            {
                // transform action execute to submit if version of channel doesn't support it.
                outboundCard = outboundCard.TransformActionExecuteToSubmit();
            }

            outboundCard.Metadata = new AdaptiveMetadata() { WebUrl = GetCurrentCardUri().AbsoluteUri };
            return outboundCard;
        }

        private async Task AddMetadataToCard(AdaptiveCard outboundCard, bool isPreview, CancellationToken cancellationToken)
        {
            var sessionId = (isPreview) ? null : this.Route.SessionId;
            string? sessionIdEncrypt = null;
            if (!String.IsNullOrEmpty(sessionId))
            {
                sessionIdEncrypt = await Context.EncryptionProvider.EncryptAsync(sessionId, cancellationToken);
            }
            var route = GetCurrentCardRoute();

            var visitor = new AdaptiveVisitor();
            visitor.Visit(outboundCard);

            foreach (var action in visitor.Elements.OfType<AdaptiveExecuteAction>())
            {
                action.Data = AddMetadataToDataPayload(action.Data, route, sessionIdEncrypt, action.Id);

                // hide goofy feedback panel
                // THIS IS DISABLED UNTIL BUG IS FIXED
                //if (Activity.ChannelId == Channels.Msteams)
                //{
                //    action.AdditionalProperties["msTeams"] = new JObject() { { "feedback", new JObject() { { "hide", true } } } };
                //}
            }

            foreach (var action in visitor.Elements.OfType<AdaptiveSubmitAction>())
            {
                action.Data = AddMetadataToDataPayload(action.Data, route, sessionIdEncrypt, action.Id);

                // hide goofy feedback panel
                // THIS IS DISABLED UNTIL BUG IS FIXED
                //if (Activity.ChannelId == Channels.Msteams)
                //{
                //    action.AdditionalProperties["msTeams"] = new JObject() { { "feedback", new JObject() { { "hide", true } } } };
                //}
            }

            foreach (var choiceSet in visitor.Elements.OfType<AdaptiveChoiceSetInput>())
            {
                if (choiceSet.DataQuery != null)
                {
                    // teams supports direct query of graph api 
                    if ((Activity!.ChannelId == Channels.Msteams || Activity!.ChannelId == Channels.Outlook) &&
                        choiceSet.DataQuery.Dataset.StartsWith("graph.microsoft.com/users"))
                        continue;
                    else
                        choiceSet.DataQuery.Dataset = $"{Route.Route}{AdaptiveDataQuery.Separator}{sessionId}{AdaptiveDataQuery.Separator}{choiceSet.DataQuery.Dataset}";
                }
            }
        }

        private static JObject AddMetadataToDataPayload(object inObj, string route, string? sessionIdEncrypt, string? id)
        {
            ArgumentNullException.ThrowIfNull(route);

            JObject data = new JObject();

            if (inObj is JObject job)
                data = job;
            else
            {
                var text = inObj?.ToString()!.Trim();
                if (!String.IsNullOrEmpty(text))
                {
                    data = JObject.Parse(text);
                }
            }

            data[Constants.ROUTE_KEY] = route;
            if (sessionIdEncrypt != null)
                data[Constants.SESSION_KEY] = sessionIdEncrypt;
            if (id != null)
                data[Constants.IDDATA_KEY] = id;
            return data;
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
                        Verb = Constants.LOAD_VERB,
                        IconUrl = new Uri(uri, Context.Configuration.GetValue<string>("RefreshIcon") ?? "/images/refresh.png").AbsoluteUri,
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
                        Verb = Constants.REFRESH_VERB,
                        IconUrl = new Uri(uri, Context.Configuration.GetValue<string>("RefreshIcon") ?? "/images/refresh.png").AbsoluteUri,
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

            if (this.Activity!.ChannelId == Channels.Msteams)
            {
                if (TeamsConversationMembers == null)
                {
                    var teamId = Activity.GetChannelData<TeamsChannelData>().Team?.Id ?? Activity.Conversation.Id;
                    try
                    {
                        // we need to add refresh userids
                        var connectorClient = Context.TurnContext.TurnState?.Get<IConnectorClient>()!;
                        var teamsMembers = await connectorClient.Conversations.GetConversationPagedMembersAsync(teamId, 60, cancellationToken: cancellationToken);
                        this.TeamsConversationMembers = teamsMembers.Members.Select(member => $"8:orgid:{member.AadObjectId}").ToList();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError($"Failed to get UserIds for conversation.id={teamId}\n{ex.Message}");
                    }
                }
                outboundCard.Refresh.UserIds = this.TeamsConversationMembers!;
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
                        Bleed = true,
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
            var currentUri = GetCurrentCardUri();

            var channelOptions = Context.ServiceOptions.GetChannelOptions(Activity.ChannelId);

            var systemMenu = new AdaptiveActionSet()
            {
                Id = "systemMenu",
                IsVisible = false,
                Separator = true
            };

            if (!IsTaskModule && !isPreview)
            {
                systemMenu.Actions.Add(new AdaptiveOpenUrlAction()
                {
                    Title = "Open",
                    IconUrl = new Uri(currentUri, Context.Configuration.GetValue<string>("OpenLinkIcon") ?? "/images/OpenLink.png").AbsoluteUri,
                    Url = currentUri,
                    Mode = AdaptiveActionMode.Secondary
                });
            }

            if (Activity.ChannelId == currentUri.Host && outboundCard.Refresh?.Action != null)
            {
                outboundCard.Refresh.Action.Mode = AdaptiveActionMode.Secondary;
                systemMenu.Actions.Add(outboundCard.Refresh.Action);
            }

            //if (Context.RouteResolver.ResolveRoute(CardRoute.Parse($"/Cards/{this.Name}/{Constants.ABOUT_VIEW}"), out var cardViewType) && this.Route.View != Constants.ABOUT_VIEW)
            //{
            //    systemMenu.Actions.Add(new AdaptiveExecuteAction()
            //    {
            //        Title = Constants.ABOUT_VIEW,
            //        Verb = Constants.ABOUT_VERB,
            //        IconUrl = new Uri(currentUri, Context.Configuration.GetValue<string>("AboutIcon") ?? "/images/about.png").AbsoluteUri,
            //        AssociatedInputs = AdaptiveAssociatedInputs.None,
            //        Mode = AdaptiveActionMode.Secondary
            //    });
            //}

            //if (Context.RouteResolver.ResolveRoute(CardRoute.Parse($"/Cards/{this.Name}/{Constants.SETTINGS_VIEW}"), out cardViewType) && this.Route.View != Constants.SETTINGS_VIEW)
            //{
            //    systemMenu.Actions.Add(new AdaptiveExecuteAction()
            //    {
            //        Title = Constants.SETTINGS_VIEW,
            //        Verb = Constants.SETTINGS_VERB,
            //        IconUrl = new Uri(currentUri, Context.Configuration.GetValue<string>("SettingsIcon") ?? "/images/settings.png").AbsoluteUri,
            //        AssociatedInputs = AdaptiveAssociatedInputs.None,
            //        Mode = AdaptiveActionMode.Secondary
            //    });
            //}

            if (Diag.Debugger.IsAttached)
            {
                systemMenu.Actions.Add(new AdaptiveExecuteAction()
                {
                    Title = "Debugger",
                    Verb = Constants.DEBUGGER_VERB,
                    AssociatedInputs = AdaptiveAssociatedInputs.Auto,
                    Mode = AdaptiveActionMode.Secondary
                });
            }

            // AddCardHeader means we simluate a card header using adaptive card markup.  Currently this is only used by "host" page for crazor.
            if (!channelOptions.SupportsCardHeader)
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

#pragma warning disable CS0618 // Type or member is obsolete
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
#pragma warning restore CS0618 // Type or member is obsolete

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
                // simply promote the actions into the outbound card.
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

        protected string? GetScopedKey(string scope, string? key)
        {
            ArgumentNullException.ThrowIfNull(scope);
            if (String.IsNullOrEmpty(key))
                return null;
            return $"{this.Name}-{scope}-{key ?? "data"}";
        }

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
                var key = GetScopedKey(memoryAttribute.Name, memoryAttribute.GetKey(this));
                if (key != null && !attributeMap.ContainsKey(key))
                {
                    attributeMap.Add(key, memoryAttribute);
                }
            }
            return attributeMap;
        }

        /// <summary>
        /// Request signin authentication for default connectionName
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task LoginUserAsync(CancellationToken cancellationToken)
        {
            if (Context.User.Identity?.IsAuthenticated == false)
            {
                var authentication = await GetAdaptiveAuthentication(cancellationToken);
                throw new AdaptiveAuthenticationRequiredException(authentication!, HttpStatusCode.Unauthorized);
            }
        }

        /// <summary>
        /// Request logout for the user.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task LogoutUserAsync(CancellationToken cancellationToken)
        {
            await LogoutUserAsync(OAuthConnectionAttribute.Default, cancellationToken);
        }

        /// <summary>
        /// Request logout for the user.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task LogoutUserAsync(string connectionName, CancellationToken cancellationToken)
        {
            var userTokenClient = Context.TurnContext.TurnState.Get<UserTokenClient>();

#pragma warning disable CS0618 // Type or member is obsolete
            if (userTokenClient != null)
            {
                await userTokenClient.SignOutUserAsync(Activity.From.Id, connectionName, Activity.ChannelId, cancellationToken);
            }
            else if (Context.TurnContext.Adapter is IExtendedUserTokenProvider adapter)
            {
                await adapter.SignOutUserAsync(Context.TurnContext, connectionName, Activity.From.Id, cancellationToken);
            }
            else
            {
                throw new NotSupportedException("Logout is not supported by the current adapter.");
            }
#pragma warning restore CS0618 // Type or member is obsolete

            Context.User = new ClaimsPrincipal(new ClaimsIdentity());
            // make sure that user is set into the authenticationStateProvider is initialized
            var isp = this.Context.AuthenticationStateProvider as IHostEnvironmentAuthenticationStateProvider;
            if (isp != null)
            {
                isp.SetAuthenticationState(Task.FromResult(new AuthenticationState(Context.User)));
            }
        }

        public virtual async Task AuthenticateUserAsync(CancellationToken cancellation)
        {
            var authenticationAttribute = CurrentView.GetType().GetCustomAttribute<OAuthConnectionAttribute>();
            if (authenticationAttribute != null && !String.IsNullOrEmpty(Activity.From.Id))
            {
                var userTokenClient = Context.TurnContext.TurnState?.Get<UserTokenClient>();
                if (userTokenClient != null)
                {
                    var tokenResponse = await userTokenClient.GetUserTokenAsync(Activity.From.Id, authenticationAttribute.Connection, Activity.ChannelId, null, default);
                    var status = await userTokenClient.GetTokenStatusAsync(Activity.From.Id, Activity.ChannelId, String.Empty, cancellation);
                    if (tokenResponse != null)
                    {
                        // parse token into identity and claims.
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var jwtSecurityToken = tokenHandler.ReadJwtToken(tokenResponse.Token);
                        var claimsIdentity = new ClaimsIdentity(jwtSecurityToken.Claims, "JWT");
                        var user = new ClaimsPrincipal(claimsIdentity);
                        var isp = this.Context.AuthenticationStateProvider as IHostEnvironmentAuthenticationStateProvider;
                        if (isp != null)
                        {
                            isp.SetAuthenticationState(Task.FromResult(new AuthenticationState(Context.User)));
                        }
                    }
                }
            }
            var state = await Context.AuthenticationStateProvider.GetAuthenticationStateAsync();
            Context.User = state.User;
        }

        /// <summary>
        /// Authorize the activity for the user.
        /// </summary>
        /// <remarks>
        /// * This will assign Context.User and Context.UserToken if the user is authorized.
        /// </remarks>
        /// <exception cref="UnauthorizedAccessException">It will throw an UnauthorizedAccessException if the authenticated user does not meet [Authorized] attribute definition.</exception>
        /// <param name="cancellationToken"></param>
        /// <returns>It will return Authentication if the user is not authenticated.</returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public virtual async Task AuthorizeActivityAsync(IInvokeActivity activity, bool isPreview, CancellationToken cancellationToken)
        {
            // If there are authentication attribute on the view, then we need to authenticate the user.
            var oauthAttribute = this.CurrentView.GetType().GetCustomAttribute<OAuthConnectionAttribute>();

            // Authorize user by processing authorize attributes
            var authorizeAttributes = this.CurrentView.GetType().GetCustomAttributes<AuthorizeAttribute>().ToList();
            if (isPreview == false && authorizeAttributes.Any())
            {
                // if we are not authenticated and there are Authorize attributes then we just blow out of here.
                if (Context.User?.Identity?.IsAuthenticated == false)
                {
                    // authentication is required.
                    var authentication = await GetAdaptiveAuthentication(cancellationToken);
                    throw new AdaptiveAuthenticationRequiredException(authentication!, HttpStatusCode.Unauthorized);
                }

                foreach (var authorizeAttribute in authorizeAttributes)
                {
                    // if we have a policy then validate it.
                    if (!String.IsNullOrEmpty(authorizeAttribute.Policy))
                    {
                        var result = await this.Context.AuthorizationService.AuthorizeAsync(Context.User!, authorizeAttribute.Policy);
                        if (result.Failure != null)
                        {
                            throw new UnauthorizedAccessException(String.Join("\n", result.Failure.FailureReasons.Select(reason => reason.Message)));
                        }
                    }

                    // if we have roles, then validate them.
                    if (!String.IsNullOrEmpty(authorizeAttribute.Roles))
                    {
                        foreach (var role in authorizeAttribute.Roles.Split(',').Select(r => r.Trim()))
                        {
                            if (!Context.User!.IsInRole(role))
                            {
                                throw new UnauthorizedAccessException($"User is not in required role [{role}]");
                            }
                        }
                    }
                }
            }
        }


        private async Task<AdaptiveAuthentication?> GetAdaptiveAuthentication(CancellationToken cancellationToken)
        {
            var oauthAttribute = this.CurrentView.GetType().GetCustomAttribute<OAuthConnectionAttribute>();
            if (oauthAttribute != null && !String.IsNullOrEmpty(Activity.From.Id))
            {
                var userTokenClient = Context.TurnContext.TurnState?.Get<UserTokenClient>();
                if (userTokenClient != null)
                {
                    var signinResource = await userTokenClient.GetSignInResourceAsync(oauthAttribute.Connection, (Activity)Activity, null, cancellationToken);

                    // create authenticationOptions to ask to be logged in.
                    var authentication = new AdaptiveAuthentication()
                    {
                        ConnectionName = oauthAttribute.Connection,
                        Text = "Please sign in to continue",
                        Buttons = new List<AdaptiveAuthCardButton>
                    {
                        new AdaptiveAuthCardButton()
                        {
                            Title = "Sign In",
                            Type = "signin",
                            Value = signinResource.SignInLink
                        }
                    },
                        TokenExchangeResource = JObject.FromObject(signinResource.TokenExchangeResource).ToObject<AdaptiveTokenExchangeResource>()!,
                    };
                    return authentication;
                }
            }
            return null!;
        }
    }
}
