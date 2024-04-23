// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using Crazor.Attributes;
using Crazor.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

namespace Crazor
{
    /// <summary>
    /// CustomCardView class, implement OnRenderCard() to do custom rendering
    /// </summary>
    public abstract class CustomCardViewBase<AppT> : ICardView
        where AppT : CardApp
    {
        private static HashSet<string> ignorePropertiesOnTypes = new HashSet<string>() { "CustomCardViewBase`1", "CustomCardView", "CustomCardView`1", "CustomCardView`2" };

        public CustomCardViewBase()
        {
        }

        /// <inheritdoc/>
        public string Name => this.GetType().Name;

        public ClaimsPrincipal? User => App.Context.User!;

        /// <inheritdoc/>
        /// <summary>
        /// App for this CardView
        /// </summary>
        public AppT App { get; set; }

        /// <summary>
        /// ICardView interface
        /// </summary>
        CardApp ICardView.App { get => App; set => App = (AppT)value; }


        /// <inheritdoc/>
        public Dictionary<string, HashSet<string>> ValidationErrors => new Dictionary<string, HashSet<string>>();

        /// <inheritdoc/>
        public bool IsModelValid { get; set; }

        /// <summary>
        /// True if the card is inside a taskmodule
        /// </summary>
        public bool IsTaskModule => App.IsTaskModule;

        /// <summary>
        /// True if the card is being rendered to be shared with people without session data
        /// </summary>
        public bool IsPreview { get; set; }

        /// <inheritdoc/>
        public virtual object? GetModel()
        {
            return null;
        }

        /// <inheritdoc/>
        public virtual void SetModel(object model)
        {

        }

        /// <inheritdoc/>
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


        /// <inheritdoc/>
        public abstract Task<AdaptiveCard?> RenderCardAsync(bool isPreview, CancellationToken cancellationToken);


        #region  ----- Action Lifecycle Methods ----

        /// <inheritdoc/>
        public virtual void OnInitialized()
        {
        }

        /// <inheritdoc/>
        public virtual async Task OnInitializedAsync(CancellationToken cancellationToken)
        {
            OnInitialized();
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async virtual Task OnActionAsync(AdaptiveCardInvokeAction action, CancellationToken cancellationToken)
        {
            await this.App.OnActionReflectionAsync(action, cancellationToken);
        }

        public virtual void OnResumeView(CardResult cardResult)
        {
        }

        /// <inheritdoc/>
        public virtual async Task OnResumeViewAsync(CardResult cardResult, CancellationToken cancellationToken)
        {
            OnResumeView(cardResult);
            await Task.CompletedTask;
        }

        public virtual AdaptiveChoice[] OnSearchChoices(SearchInvoke search)
        {
            return Array.Empty<AdaptiveChoice>();
        }

        /// <inheritdoc/>
        public virtual async Task<AdaptiveChoice[]> OnSearchChoicesAsync(SearchInvoke search, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return OnSearchChoices(search);
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
        #endregion

        /// <inheritdoc/>
        public virtual IEnumerable<PropertyInfo> GetPersistentProperties()
        {
            return this.GetType().GetProperties().Where(propertyInfo =>
            {
                if (propertyInfo.GetCustomAttribute<SessionMemoryAttribute>() != null)
                    return true;

                if (propertyInfo.GetCustomAttribute<TempMemoryAttribute>() != null)
                    return false;

                if (propertyInfo.GetCustomAttribute<InjectAttribute>() != null)
                    return false;

                if (propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                    return false;

                if (ignorePropertiesOnTypes.Contains(propertyInfo.DeclaringType.Name!))
                    return false;

                if (propertyInfo.Name == "Model")
                    return false;

                return true;

            }).ToList();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<PropertyInfo> GetBindableProperties()
        {
            return this.GetType().GetProperties().Where(propertyInfo =>
            {
                if (propertyInfo.Name == "Model")
                    return true;

                if (propertyInfo.GetCustomAttribute<SessionMemoryAttribute>() != null)
                    return true;

                if (propertyInfo.GetCustomAttribute<InjectAttribute>() != null)
                    return false;

                if (propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                    return false;

                if (ignorePropertiesOnTypes.Contains(propertyInfo.DeclaringType.Name!))
                    return false;

                return true;
            }).ToList();
        }
    }

    public abstract class CustomCardView : CustomCardViewBase<CardApp>
    {
    }

    public abstract class CustomCardView<AppT> : CustomCardViewBase<AppT>
        where AppT : CardApp
    {
    }

    public abstract class CustomCardView<AppT, ModelT> : CustomCardViewBase<AppT>
        where AppT : CardApp
        where ModelT : class
    {
        public CustomCardView()
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
