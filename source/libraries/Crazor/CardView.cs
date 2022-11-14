using AdaptiveCards;
using Crazor.Attributes;
using Crazor.Exceptions;
using Crazor.Interfaces;
using Crazor.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Xml;
using Diag = System.Diagnostics;

namespace Crazor
{
    public class CardViewBase<AppT> : RazorPage, ICardView
        where AppT : CardApp
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CardViewBase()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }

        /// <summary>
        /// UrlHelper for creating links to resources on this service.
        /// </summary>
        public IUrlHelper UrlHelper { get; set; }

        /// <summary>
        /// Name of the CardView
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// App for this CardView
        /// </summary>
        public AppT App { get; set; }

        /// <summary>
        /// IView interface
        /// </summary>
        CardApp ICardView.App { get => App; set => App = (AppT)value; }

        /// <summary>
        /// Current action being processed.
        /// </summary>
        public AdaptiveCardInvokeAction Action { get; set; }

        /// <summary>
        /// Loaded razor view for the this CardView.
        /// </summary>
        public IView RazorView { get; set; }

        /// <summary>
        /// True if the model and properites on the view have passed validation
        /// </summary>
        public bool IsModelValid { get; set; } = true;

        /// <summary>
        /// Validation Errors for current input to the cardview.
        /// </summary>
        /// <remarks>The key is the id of the input control, the hashset is the error messages.</remarks>
        public Dictionary<string, HashSet<string>> ValidationErrors { get; set; } = new Dictionary<string, HashSet<string>>();

        /// <summary>
        /// True if the card is inside a taskmodule
        /// </summary>
        public bool IsTaskModule => App.IsTaskModule;

        /// <summary>
        /// True if the card is being rendered to be shared with people without session data
        /// </summary>
        public bool IsPreview { get; set; }

        /// <summary>
        /// IView->ExecuteAsync is disabled because the default writes the output directly to 
        /// the response, and we need to process it directly.
        /// </summary>
        /// <returns></returns>
        public override Task ExecuteAsync()
        {
            return Task.CompletedTask;
        }

        #region ---- Core Methods -----
        /// <summary>
        /// OnInvokeActionAsync() - Called to process an incoming verb action.
        /// </summary>
        /// <remarks>
        /// The default implementation uses reflection to find the name of the method and invoke it.
        /// </remarks>
        /// <param name="action">the action to process</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async virtual Task OnActionAsync(AdaptiveCardInvokeAction action, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(this.App);
            this.Action = action;

            // Process BindProperty tags
            BindProperties();

            var data = JObject.FromObject(this.Action.Data);

            MethodInfo? verbMethod = null;
            if (action.Verb == Constants.LOADROUTE_VERB)
            {
                // LoadRoute verb should invoke this method FIRST before validation, as this method should load the model.
                verbMethod = GetMethod(action.Verb);
                if (verbMethod != null)
                {
                    var routeAttribute = this.GetType().GetCustomAttribute<RouteAttribute>();
                    if (routeAttribute != null)
                    {
                        var loadRoute = data.ToObject<LoadRouteModel>();
                        data = GetDataForRoute(routeAttribute, loadRoute!);
                    }

                    try
                    {
                        await InvokeMethodAsync(verbMethod, GetMethodArgs(verbMethod, data, cancellationToken));
                    }
                    catch (CardRouteNotFoundException notFound)
                    {
                        AddBannerMessage(notFound.Message, AdaptiveContainerStyle.Attention);
                        CancelView();
                    }
                    catch (Exception err)
                    {
                        if (err.InnerException is CardRouteNotFoundException notFound)
                        {
                            CancelView(notFound.Message);
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
                ValidateModel();
            }

            switch (Action.Verb)
            {
                case Constants.CANCEL_VERB:
                    // if there is an OnCancel, call it
                    if (await InvokeVerbAsync(this.Action, cancellationToken) == false)
                    {
                        // default implementation 
                        this.CancelView();
                    }
                    break;

                case Constants.OK_VERB:
                    if (await InvokeVerbAsync(this.Action, cancellationToken) == false)
                    {
                        if (IsModelValid)
                        {
                            this.CloseView(ViewContext.ViewData.Model);
                        }
                    }
                    break;

                default:
                    if (await InvokeVerbAsync(action, cancellationToken) == false)
                    {
                        // Otherwise, if a verb matches a view just navigate to it.
                        if (App.HasView(action.Verb))
                        {
                            ShowView(action.Verb);
                        }
                    }
                    break;
            }
        }


        /// <summary>
        /// Bind View to an adaptive card
        /// </summary>
        /// <remarks>Override this to do custom binding to the adaptive card.</remarks>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>bound card</returns>
        public virtual async Task<AdaptiveCard?> RenderCardAsync(bool isPreview, CancellationToken cancellationToken)
        {
            this.IsPreview = isPreview;

            string xml = String.Empty;
            try
            {
                using (StringWriter writer = new StringWriter())
                {
                    this.ViewContext.Writer = writer;
                    await this.RazorView!.RenderAsync(this.ViewContext);
                    xml = writer.ToString().Trim();
                }

                if (!String.IsNullOrWhiteSpace(xml))
                {
                    if (!xml.StartsWith("<?xml"))
                    {
                        xml = $"<?xml version=\"1.0\" encoding=\"utf-16\"?>\n{xml}";
                    }
                    Diag.Debug.WriteLine(xml);

                    var reader = XmlReader.Create(new StringReader(xml));
                    var card = (AdaptiveCard?)AdaptiveCard.XmlSerializer.Deserialize(reader);
                    return card;
                }
                else
                {
                    // no card defined in markup
                    return new AdaptiveCard("1.5");
                }
            }
            catch (Exception err)
            {
                var xerr = err as XmlException ?? err.InnerException as XmlException;
                if (xerr != null)
                {
                    var line = String.Join("\n", xml.Trim() + "\n\n".Split("\n").Skip(xerr.LineNumber - 2).Take(2));
                    throw new XmlException($"{xerr.Message}\n{line}", xerr.InnerException, xerr.LineNumber, xerr.LinePosition);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Called to initialize cardState
        /// </summary>
        /// <param name="cardState"></param>
        public virtual void LoadState(CardViewState cardState)
        {
            if (cardState.Initialized == false)
            {
                // call hook to give cardview opportunity to process data.
                OnInitialized();
                cardState.Initialized = true;
            }
        }

        /// <summary>
        /// GetRoute() - returns custom subpath for the view
        /// </summary>
        /// <remarks>
        /// Override this to define custom subroute
        /// The default is /Cards/{appName/{viewName}/{result of GetRoute()}
        /// </remarks>
        /// <returns>relative path to the card for deep linking</returns>
        public virtual string GetRoute()
        {
            return String.Empty;
        }
        #endregion -----

        #region  ----- Action Lifecycle Methods ----
        /// <summary>
        /// OnInitialized() - Initalize members
        /// </summary>
        /// <remarks>
        /// This will be called only once to initialize the instance data of the cardview.
        /// This is effectively like a constructor, with no async support.  If you
        /// want to look up data to look at OnLoadCardAsync
        /// </remarks>
        public virtual void OnInitialized()
        {
        }

        /// <summary>
        /// OnResumeView() - Called when a CardResult has returned back to this view
        /// </summary>
        /// <remarks>
        /// Override this to handle the result that is returned to the card from a child view.
        /// When a view is resumed because a child view has completed this method will
        /// be called giving you an opportunity to do something with the result of the child view.
        /// </remarks>
        /// <param name="cardResult">the card result</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>task</returns>
        public virtual async Task OnResumeView(CardResult cardResult, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }


        /// <summary>
        /// Implement this to return search results for a Search command for this view.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async virtual Task<SearchResult[]> OnSearch(MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return Array.Empty<SearchResult>();
        }

        /// <summary>
        /// Override this to provide dynamic choices for Input.ChoiceSet
        /// </summary>
        /// <param name="search">request</param>
        /// <param name="cancellationToken"></param>
        /// <returns>array of choices</returns>
        public virtual async Task<AdaptiveChoice[]> OnSearchChoices(SearchInvoke search, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return Array.Empty<AdaptiveChoice>();
        }
        #endregion

        #region ----- Utility Methods -----
        /// <summary>
        /// Add a banner message to be displayed to the viewer.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="style"></param>
        public void AddBannerMessage(string text, AdaptiveContainerStyle style = AdaptiveContainerStyle.Default)
        {
            this.App?.AddBannerMessage(text, style);
        }

        /// <summary>
        /// Navigate to card by name
        /// </summary>
        /// <param name="cardName">name of card </param>
        /// <param name="model">model to pass</param>
        public void ShowView(string cardName, object? model = null)
        {
            this.App!.ShowView(cardName, model);
        }

        /// <summary>
        /// Navigate to view by type
        /// </summary>
        /// <typeparam name="T">type of the object to navigate to</typeparam>
        /// <param name="model">model</param>
        public void ShowView<T>(object? model = null)
        {
            this.App!.ShowView(typeof(T).FullName!, model);
        }

        /// <summary>
        /// Replace this view with another one 
        /// </summary>
        /// <param name="cardName"></param>
        /// <param name="model">model to pass</param>
        public void ReplaceView(string cardName, object? model = null)
        {
            this.App!.ReplaceView(cardName, model);
        }

        /// <summary>
        /// Replace this view with another one 
        /// </summary>
        /// <typeparam name="T">Type to instantiate</typeparam>
        /// <param name="model">model to pass</param>
        public void ReplaceView<T>(object? model = null)
        {
            this.App!.ReplaceView(typeof(T).FullName!, model);
        }

        /// <summary>
        /// Close the current card, optionalling returning the result
        /// </summary>
        /// <param name="result">the result to return to the current caller</param>
        public void CloseView(object? result = null)
        {
            this.App?.CloseView(new CardResult()
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
            this.App?.CloseView(new CardResult()
            {
                Name = this.Name,
                Message = message,
                Success = false
            });
        }

        /// <summary>
        /// Change the taskmodule status
        /// </summary>
        /// <param name="status">action to take on closing</param>
        public void CloseTaskModule(TaskModuleAction status)
        {
            App.CloseTaskModule(status);
        }

        #endregion

        #region ---- Private Methods ----
        private async Task<bool> InvokeVerbAsync(AdaptiveCardInvokeAction action, CancellationToken cancellationToken)
        {
            var verbMethod = GetMethod(action.Verb);
            if (verbMethod != null)
            {
                await InvokeMethodAsync(verbMethod, GetMethodArgs(verbMethod, (JObject?)this.Action?.Data, cancellationToken));
                return true;
            }
            return false;
        }

        private static List<object?>? GetMethodArgs(MethodInfo? method, JObject? data, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(method);

            List<object?> args = new List<object?>();
            if (data != null)
            {
                foreach (var parm in method.GetParameters())
                {
                    if (parm.ParameterType == typeof(CancellationToken))
                    {
                        args.Add(cancellationToken);
                    }
                    //else if (parm.Name?.ToLower() == "id")
                    //{
                    //    if (Action!.Id != null)
                    //    {
                    //        args.Add(Action.Id);
                    //    }
                    //    else if (data.TryGetValue(Constants.IDDATA_KEY, out var id))
                    //    {
                    //        args.Add(id.ToString());
                    //    }
                    //}
                    else
                    {
                        var prop = data.Properties().Where(p => p.Name.ToLower() == parm?.Name?.ToLower()).SingleOrDefault();
                        if (prop != null)
                        {
                            var arg = prop.Value.ToObject(parm.ParameterType);
                            args.Add(arg);
                        }
                        else
                        {
                            args.Add(parm.ParameterType.IsValueType ? Activator.CreateInstance(parm.ParameterType) : null);
                        }
                    }
                }
            }
            return args;
        }

        private async Task<object?> InvokeMethodAsync(MethodInfo? verbMethod, List<object?>? args = null)
        {
            ArgumentNullException.ThrowIfNull(verbMethod);

            if (verbMethod.ReturnType.Name == "Task")
            {
                await ((Task?)verbMethod.Invoke(this, args?.ToArray()) ?? throw new Exception("Task not returned from async verb!"));
                return null;
            }
            else if (verbMethod.ReturnType.Name == "Task`1")
            {
                var task = verbMethod.Invoke(this, args?.ToArray());
                if (task != null)
                {

                    await (Task)task;
                    var result = task!.GetType().GetProperty("Result", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance)!.GetValue(task);
                    return result;
                }
                throw new ArgumentNullException(verbMethod.Name);
            }
            else
            {
                return verbMethod.Invoke(this, args?.ToArray());
            }

        }
        private MethodInfo? GetMethod(string methodName)
        {
            return this.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                ?? this.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        }

        private void BindProperties()
        {
            JObject? data = (JObject?)this.Action?.Data;
            if (data != null)
            {
                foreach (var property in data.Properties())
                {
                    var parts = property.Name.Split('.');

                    // if root is [BindProperty]
                    var prop = this.GetType().GetProperty(parts[0]);
                    if (prop != null &&
                        (prop.Name == "Model" || prop.Name == "App" || prop.GetCustomAttribute<BindPropertyAttribute>() != null))
                    {
                        object obj = this;
                        foreach (var part in parts.Take(parts.Length - 1))
                        {
                            obj = obj.GetPropertyValue(part)!;
                        }
                        var targetProperty = obj.GetType().GetProperty(parts.Last());
                        obj.SetTargetProperty(targetProperty, property.Value);
                    }
                }
            }
        }

        private void ValidateModel()
        {
            // validate root object model
            var validator = new DataAnnotationsValidator();

            // do shallow validation for root level properties
            var validationResults = new List<ValidationResult>();
            this.IsModelValid = validator.TryValidateObject(this, validationResults);
            AddValidationResults(String.Empty, validationResults);

            // for complex types do a recursive deep validation. We can't
            // do this at the root because CardView is too complicated for a deep compare.
            foreach (var property in this.GetType().GetProperties()
                                        .Where(p => (p.GetCustomAttribute<BindPropertyAttribute>() != null || p.GetCustomAttribute<SessionMemoryAttribute>() != null))
                                        .Where(p => !p.PropertyType.IsValueType && p.PropertyType != typeof(string)))
            {
                validationResults = new List<ValidationResult>();
                var value = property.GetValue(this);
                if (!validator.TryValidateObjectRecursive(value, validationResults))
                {
                    this.IsModelValid = false;
                    AddValidationResults($"{property.Name}.", validationResults);
                }
            }
        }

        private void AddValidationResults(string prefix, List<ValidationResult> validationResults)
        {
            foreach (var result in validationResults)
            {
                foreach (var member in result.MemberNames)
                {
                    var path = $"{prefix}{member}";
                    if (!ValidationErrors.TryGetValue(path, out var list))
                    {
                        list = new HashSet<string>();
                        ValidationErrors[path] = list;
                    }

                    if (result.ErrorMessage != null)
                    {
                        list.Add(result.ErrorMessage);
                    }
                }
            }
        }

        private JObject GetDataForRoute(RouteAttribute route, LoadRouteModel loadRoute)
        {
            JObject result = new JObject();
            result["view"] = loadRoute.View;
            result["path"] = loadRoute.Path;
            string path = loadRoute.Path.Replace(this.App.GetCurrentCardRoute(), String.Empty);

            if (!String.IsNullOrEmpty(path))
            {
                var dataParts = path!.TrimStart('/').Split('?');
                var dataPathParts = dataParts.First().Split('/');
                var dataQuery = dataParts.Skip(1).FirstOrDefault();
                var templateParts = route.Template.Split('?');
                int i = 0;
                foreach (var fragment in templateParts[0].Split('/'))
                {
                    if (fragment.StartsWith('{') && fragment.EndsWith('}'))
                    {
                        var name = fragment.TrimStart('{').TrimEnd('}', '?');
                        result[name] = dataPathParts[i];
                    }
                    i++;
                }
            }
            return result;
        }
        #endregion
    }

    public class CardView : CardViewBase<CardApp>
    {
    }

    public class CardView<AppT> : CardViewBase<AppT>
        where AppT : CardApp
    {
    }

    public class CardView<AppT, ModelT> : CardViewBase<AppT>
        where AppT : CardApp
        where ModelT : class
    {
        public CardView()
        {
            if (typeof(ModelT).IsAssignableTo(typeof(CardApp)))
            {
                throw new ArgumentException($"{nameof(ModelT)} You can't pass a CardApp as a model");
            }
        }

        // Summary:
        //     Gets the Model property of the Microsoft.AspNetCore.Mvc.Razor.RazorPage`1.ViewData
        //     property.
        [SessionMemory]
        public ModelT Model
        {
            get
            {
                if (ViewData?.Model != null)
                {
                    return ViewData.Model;
                }

#pragma warning disable CS8603 // Possible null reference return.
                return default(ModelT);
#pragma warning restore CS8603 // Possible null reference return.
            }

            set
            {
                this.ViewData!.Model = value;
                this.ViewContext.ViewData.Model = value;
            }
        }

        //
        // Summary:
        //     Gets or sets the dictionary for view data.
        [RazorInject]
        public ViewDataDictionary<ModelT>? ViewData { get; set; }

        public override void LoadState(CardViewState cardViewState)
        {
            ModelT? model = this.ViewContext.ViewData.Model as ModelT;
            if (model == null)
            {
                if (this.ViewContext.ViewData.Model is JToken jt)
                {
                    this.ViewContext.ViewData.Model = (ModelT?)jt.ToObject(typeof(ModelT));
                }
                else
                {
                    this.ViewContext.ViewData.Model = Activator.CreateInstance<ModelT>();
                }
            }
            this.ViewData = new ViewDataDictionary<ModelT>(this.ViewContext.ViewData);

            base.LoadState(cardViewState);
        }
    }
}
