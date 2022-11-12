using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Schema;
using AdaptiveCards;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Bot.Schema.Teams;

namespace Crazor.Interfaces
{
    public interface ICardView : IRazorPage
    {
        IUrlHelper UrlHelper { get; set; }

        string Name { get; set; }

        CardApp App { get; set; }

        AdaptiveCardInvokeAction Action { get; set; }

        IView RazorView { get; set; }

        Dictionary<string, HashSet<string>> ValidationErrors { get; set; }

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
        /// Called to get searchresults for a cardview.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<SearchResult[]> OnSearch(MessagingExtensionQuery query, CancellationToken cancellationToken);

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
