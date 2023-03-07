// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Crazor.Server
{
    public partial class CardActivityHandler
    {
        protected async override Task<AdaptiveCardInvokeResponse> OnAdaptiveCardInvokeAsync(ITurnContext<IInvokeActivity> turnContext, AdaptiveCardInvokeValue invokeValue, CancellationToken cancellationToken)
        {
            CardRoute cardRoute = await CardRoute.FromDataAsync((JObject)invokeValue.Action.Data, Context.EncryptionProvider, cancellationToken);

            var cardApp = Context.CardAppFactory.Create(cardRoute, turnContext);

            AdaptiveCard card = await cardApp.ProcessInvokeActivity((Activity)turnContext.Activity!, isPreview: false, cancellationToken);

            return new AdaptiveCardInvokeResponse()
            {
                StatusCode = 200,
                Type = AdaptiveCard.ContentType,
                Value = card
            };
        }
    }
}