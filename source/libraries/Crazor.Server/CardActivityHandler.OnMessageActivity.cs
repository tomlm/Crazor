// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
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

                        var card = await cardApp.ProcessInvokeActivity(turnContext.Activity.CreateLoadRouteActivity(cardRoute.Route), isPreview: true, cancellationToken);

                        var response = Activity.CreateMessageActivity();
                        response.Attachments.Add(new Attachment(AdaptiveCard.ContentType, content: card));

                        await turnContext.SendActivityAsync(response, cancellationToken);
                    }
                }
            }
        }

    }
}