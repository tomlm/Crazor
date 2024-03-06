//#define XML_SERIALIZATION
// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.AdaptiveCards;
using Crazor.Attributes;
using Crazor.Blazor.ComponentRenderer;
using Crazor.Blazor.Components;
using Crazor.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Xml;

namespace Crazor.Blazor
{
    public abstract class CardViewBase<AppT> : ComponentBase, ICardView
        where AppT : CardApp
    {
        private static HashSet<string> ignorePropertiesOnTypes = new HashSet<string>() { "CardViewBase`1", "CardView", "CardView`1", "CardView`2", "ComponentBase" };


        [Inject]
        public IServiceProvider? ServiceProvider { get; set; }

        /// <summary>
        /// Name of the CardView
        /// </summary>
        public string Name => this.GetType().Name;

        public ClaimsPrincipal? User => App.Context.User!;

        /// <summary>
        /// App for this CardView
        /// </summary>
        public AppT App { get; set; }

        /// <summary>
        /// ICardView interface
        /// </summary>
        CardApp ICardView.App { get => App; set => App = (AppT)value; }

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

        #region ---- Core Methods -----

        public override Task SetParametersAsync(ParameterView parameters)
        {
            // we do our own parameter setting because base class will reject any properties 
            // without [Parameter] on them, but we want to pass all of our state through to the view
            // that's being rendered
            foreach (var parm in parameters)
            {
                this.SetTargetProperty(this.GetType().GetProperty(parm.Name), parm.Value);
            }
            this.StateHasChanged();
            return Task.CompletedTask;
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
            await this.App.OnActionReflectionAsync(action, cancellationToken);
        }

        /// <summary>
        /// Bind View to an adaptive card
        /// </summary>
        /// <remarks>Override this to do custom binding to the adaptive card.</remarks>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>bound card</returns>
        public virtual async Task<AdaptiveCard?> RenderCardAsync(bool isPreview, CancellationToken cancellationToken)
        {
            IsPreview = isPreview;

            string xml = string.Empty;
            try
            {
                // Create a RenderFragment from the component
                var ctx = new RenderingContext(ServiceProvider);
                var renderer = ctx.RenderComponent(typeof(CardViewWrapper), ComponentParameter.CreateParameter("CardView", this));
#if XML_SERIALIZATION
                xml = rendered.Markup;

                if (!String.IsNullOrWhiteSpace(xml))
                {
                    if (!xml.StartsWith("<?xml"))
                    {
                        xml = $"<?xml version=\"1.0\" encoding=\"utf-16\"?>\n{xml}";
                    }
                    // File.WriteAllText(@"c:\scratch\foo.xml", xml);
                    System.Diagnostics.Debug.WriteLine(xml);

                    var reader = XmlReader.Create(new StringReader(xml));
                    var card = (AdaptiveCard?)AdaptiveCard.XmlSerializer.Deserialize(reader);
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debug.WriteLine(card.ToXml());
                    }
                    return card;
                }
                else
                {
                    // no card defined in markup
                    return new AdaptiveCard("1.5");
                }
#else
                // use razor in memory object instead of serialization.  The instance is a CardViewWrapper
                // which has the adaptive card already instantiated in memory and ready to go.
                var adaptiveCard = renderer.GetComponent<Components.Adaptive.Card>().Item;
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debug.WriteLine(adaptiveCard.ToXml());
                    System.Diagnostics.Debug.WriteLine(adaptiveCard.ToJson());
                }
                return adaptiveCard;
#endif
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
        /// <param name="cardRoute">route of card </param>
        /// <param name="model">model to pass</param>
        public void ShowView(string cardRoute, object? model = null)
        {
            this.App!.ShowView(cardRoute, model);
        }

        /// <summary>
        /// Replace this view with another one 
        /// </summary>
        /// <param name="cardRoute">route of card</param>
        /// <param name="model">model to pass</param>
        public void ReplaceView(string cardRoute, object? model = null)
        {
            this.App!.ReplaceView(cardRoute, model);
        }

        /// <summary>
        /// Close the current card, optionalling returning the result
        /// </summary>
        /// <param name="result">the result to return to the current caller</param>
        public void CloseView(object? result = null)
        {
            this.App?.CloseView(result);
        }

        /// <summary>
        /// Cancel the current card, returning a message
        /// </summary>
        /// <param name="message">optional message to return.</param>
        public void CancelView(string? message = null)
        {
            this.App?.CancelView(message);
        }

        /// <summary>
        /// Change the taskmodule status
        /// </summary>
        /// <param name="status">action to take on closing</param>
        public void CloseTaskModule(TaskModuleAction status)
        {
            this.App.CloseTaskModule(status);
        }

        public IEnumerable<PropertyInfo> GetPersistentProperties()
        {
            return this.GetType().GetProperties().Where(propertyInfo =>
            {
                if (propertyInfo.GetCustomAttribute<SessionMemoryAttribute>() != null)
                    return true;

                if (propertyInfo.GetCustomAttribute<TempMemoryAttribute>() != null)
                    return false;

                if (propertyInfo.GetCustomAttribute<InjectAttribute>() != null)
                    return false;

                if (ignorePropertiesOnTypes.Contains(propertyInfo.DeclaringType.Name!))
                    return false;

                if (propertyInfo.Name == "Model")
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

                if (propertyInfo.GetCustomAttribute<InjectAttribute>() != null)
                    return false;

                if (ignorePropertiesOnTypes.Contains(propertyInfo.DeclaringType.Name!))
                    return false;

                return true;
            }).ToList();
        }

        public virtual object? GetModel()
        {
            return null;
        }

        public virtual void SetModel(object? model)
        {

        }

        protected override Task OnInitializedAsync()
        {
            OnInitialized();
            return Task.CompletedTask;
        }

        Task ICardView.OnInitializedAsync()
        {
            return this.OnInitializedAsync();
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

        [SessionMemory]
        public ModelT Model { get; set; } = default!;

        public override object? GetModel()
        {
            return this.Model;
        }

        public override void SetModel(object? model)
        {
            if (model == null)
            {
                Model = Activator.CreateInstance<ModelT>();
            }
            else
            {
                Model = model as ModelT ?? JObject.FromObject(model).ToObject<ModelT>();
            }
        }
    }
}
