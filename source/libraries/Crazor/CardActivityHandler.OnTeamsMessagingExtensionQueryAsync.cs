// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Crazor
{
    public partial class CardActivityHandler
    {
        /// <summary>
        /// Handle Fetch Task request
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(
            ITurnContext<IInvokeActivity> turnContext, 
            MessagingExtensionQuery query, 
            CancellationToken cancellationToken)
        {
            _logger!.LogInformation($"Starting OnTeamsMessagingExtensionQueryAsync() processing");

            var uri = new Uri(_configuration.GetValue<Uri>("HostUri"), query.CommandId);

            CardRoute cardRoute = CardRoute.FromUri(uri);

            var cardApp = _cardAppFactory.Create(cardRoute);

            await cardApp.LoadAppAsync((Activity)turnContext.Activity, cancellationToken);

            var result = await cardApp.OnMessagingExtensionQueryAsync(query, cancellationToken);

            // don't save session data, it's a preview
            cardApp.Route.SessionId = null;
            
            await cardApp.SaveAppAsync(cancellationToken);

            return result;
        }
    }
}