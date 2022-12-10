// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.AspNetCore.Components;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Crazor.Interfaces
{
    public interface ICardView 
    {
        /// <summary>
        /// Name of the template
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// App reference
        /// </summary>
        CardApp App { get; set; }

        /// <summary>
        /// Current action
        /// </summary>
        AdaptiveCardInvokeAction Action { get; set; }

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
        /// Bind data to view properties.
        /// </summary>
        /// <param name="data"></param>
        void BindProperties(JObject data);

        /// <summary>
        /// Bind the view to the card
        /// </summary>
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
