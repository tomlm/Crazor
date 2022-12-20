// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Crazor.Interfaces
{
    public interface ICardView 
    {
        /// <summary>
        /// Name of the template
        /// </summary>
        string Name { get; }

        /// <summary>
        /// App reference
        /// </summary>
        CardApp App { get; set; }

        /// <summary>
        /// Validation errors for the current view.
        /// </summary>
        Dictionary<string, HashSet<string>> ValidationErrors { get; set; }

        /// <summary>
        /// Is the current view valid?
        /// </summary>
        bool IsModelValid { get; set; }

        /// <summary>
        /// Get any custom route data for the cardview
        /// </summary>
        /// <returns></returns>
        string GetRoute();

        /// <summary>
        /// Called to load state
        /// </summary>
        /// <param name="cardState"></param>
        void LoadState(CardViewState cardState);

        /// <summary>
        /// Called to save the card state
        /// </summary>
        /// <param name="cardState"></param>
        void SaveState(CardViewState cardState);

        /// <summary>
        /// Bind data to view properties.
        /// </summary>
        /// <param name="data"></param>
        void BindProperties(JObject data);

        /// <summary>
        /// Enumerate properties on the view which are persistent
        /// </summary>
        /// <returns></returns>
        IEnumerable<PropertyInfo> GetPersistentProperties();


        /// <summary>
        /// Enumerate properties on the view which are bindable and persistent
        /// </summary>
        /// <returns></returns>
        IEnumerable<PropertyInfo> GetBindableProperties();

        /// <summary>
        /// Render the card 
        /// </summary>
        /// <param name="isPreview">IsPreview is signal that anonymous preview card should be returned.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<AdaptiveCard?> RenderCardAsync(bool isPreview, CancellationToken cancellationToken);

        /// <summary>
        /// Called to process an InvokeAction (aka a verb)
        /// </summary>
        /// <param name="action">action payload</param>
        /// <param name="cancellationToken">cancellation token</param>
        Task OnActionAsync(AdaptiveCardInvokeAction action, CancellationToken cancellationToken);

        /// <summary>
        /// Called to process card result from the resumption of a view.
        /// </summary>
        /// <param name="screenResult"></param>
        /// <param name="cancellationToken"></param>
        Task OnResumeView(CardResult screenResult, CancellationToken cancellationToken);

        /// <summary>
        /// Called to search for choices.
        /// </summary>
        /// <param name="search"></param>
        /// <param name="services"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<AdaptiveChoice[]> OnSearchChoices(SearchInvoke search, CancellationToken cancellationToken);
    }
}
