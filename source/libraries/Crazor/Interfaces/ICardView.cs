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

        void OnLoadCardContext(ViewContext viewContext);

        Task<bool> OnVerbAsync(AdaptiveCardInvokeAction action, CancellationToken cancellationToken);

        Task OnCardResumeAsync(CardResult screenResult, CancellationToken ct);

        Task<AdaptiveCard?> BindView(IServiceProvider services);

        Task<AdaptiveChoice[]> OnSearchChoicesAsync(SearchInvoke search, IServiceProvider services);

        string GetRoute();
    }
}
