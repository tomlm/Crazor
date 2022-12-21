// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveCards;
using Crazor.Attributes;
using Crazor.Exceptions;
using Crazor.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Diag = System.Diagnostics;

namespace Crazor.Mvc
{
    /// <summary>
    /// CardView which uses MVC IRazorViewEngine to render adaptive cards.
    /// </summary>
    /// <typeparam name="AppT"></typeparam>
    public class CardViewBase<AppT> : RazorPage, IRazorPage, IMvcCardView
        where AppT : CardApp
    {
        private static HashSet<string> ignorePropertiesOnTypes = new HashSet<string>() { "CardViewBase`1", "CardView", "CardView`1", "CardView`2", "RazorPage", "RazorPageBase" };

        public static IEnumerable<Type> GetCardViewTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm =>
                asm.DefinedTypes
                    .Where(t => t.IsAbstract == false && t.ImplementedInterfaces.Contains(typeof(ICardView)))
                    .Where(t => (t.Name != "CardView" && t.Name != "CardView`1" && t.Name != "CardView`2" && t.Name != "CardViewBase`1" && t.Name != "EmptyCardView"))).ToList();
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CardViewBase()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }

        /// <summary>
        /// Name of the CardView
        /// </summary>
        public string Name => this.GetType().Name.Split('_').Last();

        /// <summary>
        /// Razor view
        /// </summary>
        public IView RazorView { get; set; }

        /// <summary>
        /// App reference
        /// </summary>
        CardApp ICardView.App { get => App; set => App = (AppT)value; }

        public AppT App { get; set; }
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

        public void BindProperties(JObject data)
        {
            if (data != null)
            {
                foreach (var property in data.Properties())
                {
                    var parts = property.Name.Split('.');

                    // if root is [BindProperty]
                    var prop = this.GetType().GetProperty(parts[0]);
                    // only allow binding to Model, App or BindProperty
                    if (prop != null && this.GetBindableProperties().Contains(prop))
                    {
                        ObjectPath.SetPathValue(this, property.Name, property.Value, json: false);
                    }
                }
            }
        }

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

            // Process BindProperty tags
            var data = (JObject)JObject.FromObject(action.Data).DeepClone();
            this.BindProperties(data);

            MethodInfo? verbMethod = null;
            if (action.Verb == Constants.LOADROUTE_VERB)
            {
                // merge in route and query data since we are in a LOAD ROUTE situation.
                if (App.Route.RouteData != null)
                    data.Merge(App.Route.RouteData);

                if (App.Route.QueryData != null)
                    data.Merge(App.Route.QueryData);

                // process [FromRoute] attributes. This allows [FromRoute] to be placed on a property which doesn't match the RouteData.property name
                foreach (var targetProperty in this.GetType().GetProperties().Where(prop => prop.GetCustomAttribute<FromRouteAttribute>() != null))
                {
                    var fromRouteName = targetProperty.GetCustomAttribute<FromRouteAttribute>().Name ?? targetProperty.Name;
                    var dataProperty = App.Route.RouteData.Properties().Where(p => p.Name.ToLower() == fromRouteName.ToLower()).SingleOrDefault();
                    if (dataProperty != null)
                    {
                        this.SetTargetProperty(targetProperty, dataProperty.Value);
                    }
                }

                // Process any Route properties as setters onto target object.
                foreach (var routeProperty in App.Route.RouteData.Properties().Where(p => !p.Name.StartsWith("App.")))
                {
                    ObjectPath.SetPathValue(this, routeProperty.Name, routeProperty.Value.ToString(), false);
                }

                // process [FromQuery] attributes
                foreach (var targetProperty in this.GetType().GetProperties().Where(a => a.GetCustomAttribute<FromQueryAttribute>() != null))
                {
                    var dataProperty = App.Route.QueryData.Properties().Where(p => p.Name.ToLower() == targetProperty.Name.ToLower()).SingleOrDefault();
                    if (dataProperty != null)
                    {
                        this.SetTargetProperty(targetProperty, dataProperty.Value);
                    }
                }

                // LoadRoute verb should invoke this method FIRST before validation, as this method should load the model.
                verbMethod = this.GetMethod(action.Verb);
                if (verbMethod != null)
                {
                    try
                    {
                        await this.InvokeMethodAsync(verbMethod, this.GetMethodArgs(verbMethod, data, cancellationToken));
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
                this.Validate();
            }

            switch (action.Verb)
            {
                case Constants.CANCEL_VERB:
                    // if there is an OnCancel, call it
                    if (await this.InvokeVerbAsync(action, cancellationToken) == false)
                    {
                        // default implementation 
                        this.CancelView();
                    }
                    break;

                case Constants.OK_VERB:
                    if (await this.InvokeVerbAsync(action, cancellationToken) == false)
                    {
                        if (IsModelValid)
                        {
                            this.CloseView(ViewContext.ViewData.Model);
                        }
                    }
                    break;

                default:
                    await this.InvokeVerbAsync(action, cancellationToken);
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
            if (cardState.Model != null)
            {
                this.ViewContext.ViewData.Model = cardState.Model;
            }

            foreach (var property in this.GetPersistentProperties())
            {
                if (cardState.SessionMemory.TryGetValue(property.Name, out var val))
                {
                    this.SetTargetProperty(property, val);
                }
            }

            if (cardState.Initialized == false)
            {
                // call hook to give cardview opportunity to process data.
                OnInitialized();
                cardState.Initialized = true;
            }
        }


        public virtual void SaveState(CardViewState state)
        {
            // capture all properties on CardView which are not on base type and not ignored.
            foreach (var property in this.GetPersistentProperties())
            {
                var val = property.GetValue(this);
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

        /// <summary>
        /// GetRoute() - returns custom subpath for the view
        /// </summary>
        /// <remarks>
        /// Override this to define custom subroute
        /// The default is to use reflection and [Route] to calculate the route
        /// </remarks>
        /// <returns>relative path to the card for deep linking</returns>
        public virtual string GetRoute()
        {
            var routeAttr = this.GetType().GetCustomAttribute<CardRouteAttribute>();
            if (routeAttr != null)
            {
                StringBuilder sb = new StringBuilder();
                var parts = routeAttr.Template.Split('/');
                for (int i = 0; i < parts.Length; i++)
                {
                    var part = parts[i];
                    if (part.StartsWith('{') && part.EndsWith('}'))
                    {
                        parts[i] = ObjectPath.GetPathValue<string>(this, part.Trim('{', '}', '?'), null);
                    }
                }
                return String.Join('/', parts);
            }
            else
            {
                return (this.Name != Constants.DEFAULT_VIEW) ? this.Name : String.Empty;
            }
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
        protected virtual void OnInitialized()
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


        #region Utility
        /// <summary>
        /// Add a banner message to be displayed to the viewer.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="style"></param>
        public void AddBannerMessage(string text, AdaptiveContainerStyle style = AdaptiveContainerStyle.Default)
        {
            this.App!.AddBannerMessage(text, style);
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
        /// Replace this view with another one 
        /// </summary>
        /// <param name="cardName"></param>
        /// <param name="model">model to pass</param>
        public void ReplaceView(string cardName, object? model = null)
        {
            this.App!.ReplaceView(cardName, model);
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
            this.App.CloseTaskModule(status);
        }
        #endregion

        public IEnumerable<PropertyInfo> GetPersistentProperties()
        {
            return this.GetType().GetProperties().Where(propertyInfo =>
            {
                if (propertyInfo.GetCustomAttribute<SessionMemoryAttribute>() != null)
                    return true;

                if (propertyInfo.GetCustomAttribute<TempMemoryAttribute>() != null)
                    return false;

                if (propertyInfo.GetCustomAttribute<RazorInjectAttribute>() != null)
                    return false;

                if (ignorePropertiesOnTypes.Contains(propertyInfo.DeclaringType.Name!))
                    return false;

                return true;

            }).ToList();
        }

        public IEnumerable<PropertyInfo> GetBindableProperties()
        {
            return this.GetType().GetProperties().Where(propertyInfo =>
            {
                if (propertyInfo.Name == "Model")
                    return true;

                if (propertyInfo.GetCustomAttribute<SessionMemoryAttribute>() != null)
                    return true;

                if (propertyInfo.GetCustomAttribute<RazorInjectAttribute>() != null)
                    return false;

                if (ignorePropertiesOnTypes.Contains(propertyInfo.DeclaringType.Name!))
                    return false;

                return true;
            }).ToList();
        }

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
            base.LoadState(cardViewState);

            ModelT? model = cardViewState.Model as ModelT;
            if (model == null)
            {
                if (cardViewState.Model is JToken jt)
                {
                    model = (ModelT?)jt.ToObject(typeof(ModelT));
                }
                else
                {
                    model = Activator.CreateInstance<ModelT>();
                }
            }

            this.ViewContext.ViewData.Model = model;
            this.ViewData = new ViewDataDictionary<ModelT>(this.ViewContext.ViewData, model);
        }
    }
}
