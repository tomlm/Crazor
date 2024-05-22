using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Crazor.Server
{
    public partial class CardActivityHandler
    {
        /// <summary>
        /// Process user typed messages
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            IMessageActivity message = turnContext.Activity.AsMessageActivity();
            if (message != null)
            {
                var input = turnContext.Activity.RemoveRecipientMention()?.Trim() ?? String.Empty;
                if (input.Contains("post"))
                {
                    var app = input.Replace("post", "").Trim();

                    if (Context.CardAppFactory.GetNames().Any(name => name.ToLower() == app.ToLower()))
                    {
                        var cardRoute = CardRoute.Parse($"/Cards/{app}");

                        var cardApp = Context.CardAppFactory.Create(cardRoute, turnContext);

                        var activity = turnContext.Activity.CreateLoadRouteActivity(cardRoute.Route);

                        await cardApp.LoadAppAsync(activity!, default);

                        var card = await cardApp.ProcessInvokeActivity(activity, isPreview: true, cancellationToken);

                        var result = await turnContext.ReplyWithCardAsync("", card, cancellationToken);
                    }
                }
            }
        }

    }
}