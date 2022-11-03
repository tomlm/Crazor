using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Schema;
using AdaptiveCards;
using Microsoft.AspNetCore.Mvc.Razor;

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
        /// Called to load state
        /// </summary>
        /// <param name="cardState"></param>
        void OnLoadCard(CardViewState cardState);

        /// <summary>
        /// Called to process verb
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> OnVerbAsync(AdaptiveCardInvokeAction action, CancellationToken cancellationToken);

        /// <summary>
        /// Called to procceses resumption
        /// </summary>
        /// <param name="screenResult"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task OnCardResumeAsync(CardResult screenResult, CancellationToken cancellationToken);

        /// <summary>
        /// Bind the view to the card
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<AdaptiveCard?> BindCard(CancellationToken cancellationToken);

        /// <summary>
        /// Called to search for choices.
        /// </summary>
        /// <param name="search"></param>
        /// <param name="services"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<AdaptiveChoice[]> OnSearchChoicesAsync(SearchInvoke search, IServiceProvider services, CancellationToken cancellationToken);

        string GetRoute();
    }
}
