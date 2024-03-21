// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor.AdaptiveCards;
using Crazor.Attributes;
using Crazor.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Crazor
{
    /// <summary>
    /// CustomCardView class, implement OnRenderCard() to do custom rendering
    /// </summary>
    public abstract class CustomCardView : ICardView
    {
        private static HashSet<string> ignorePropertiesOnTypes = new HashSet<string>() { "CustomCardView" };

        public CustomCardView()
        {
        }

        /// <inheritdoc/>
        [TempMemory]
        public string Name => this.GetType().Name;

        /// <inheritdoc/>
        [TempMemory]
        public CardApp App { get; set; }

        /// <inheritdoc/>
        [TempMemory]
        public Dictionary<string, HashSet<string>> ValidationErrors => new Dictionary<string, HashSet<string>>();

        /// <inheritdoc/>
        [TempMemory]
        public bool IsModelValid { get; set; }

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

                if (ignorePropertiesOnTypes.Contains(propertyInfo.DeclaringType.Name!))
                    return false;

                return true;
            }).ToList();
        }

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
        public async virtual Task OnActionAsync(AdaptiveCardInvokeAction action, CancellationToken cancellationToken)
        {
            await this.App.OnActionReflectionAsync(action, cancellationToken);
        }

        /// <inheritdoc/>
        protected virtual Task OnInitializedAsync()
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual async Task OnResumeView(CardResult cardResult, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }


        /// <inheritdoc/>
        public virtual async Task<AdaptiveChoice[]> OnSearchChoices(SearchInvoke search, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return Array.Empty<AdaptiveChoice>();
        }

        /// <inheritdoc/>
        public abstract Task<AdaptiveCard?> RenderCardAsync(bool isPreview, CancellationToken cancellationToken);

        /// <inheritdoc/>
        Task ICardView.OnInitializedAsync()
        {
            return this.OnInitializedAsync();
        }

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
    }
}
