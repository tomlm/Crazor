using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Crazor.Server
{
    public partial class CardActivityHandler
    {
        protected virtual async Task<AdaptiveCardInvokeResponse> OnSearchInvokeAsync(ITurnContext<IInvokeActivity> turnContext, SearchInvoke searchInvoke, CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine($"Starting application/search processing ");

            // Get session data from the invoke payload
            var parts = searchInvoke!.Dataset!.Split(AdaptiveDataQuery.Separator);
            var cardRoute = CardRoute.Parse(parts[0]);
            cardRoute.SessionId = await Context.EncryptionProvider.DecryptAsync(parts[1], cancellationToken);
            searchInvoke.Dataset = parts[2];

            var cardApp = Context.CardAppFactory.Create(cardRoute, turnContext);

            await cardApp.LoadAppAsync((Activity)turnContext.Activity, cancellationToken);

            var result = await cardApp.OnSearchChoicesAsync(searchInvoke, cancellationToken);

            await cardApp.SaveAppAsync(cancellationToken);

            return result;
        }
    }
}