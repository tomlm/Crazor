// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveCards;
using Crazor.Interfaces;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Crazor
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class SearchInvoke : SearchInvokeValue
    {
        [JsonProperty]
        public string? Dataset { get; set; }

        [JsonProperty]
        public string? SessionData { get; set; }
    }

    public partial class CardActivityHandler
    {
        protected virtual async Task<AdaptiveCardInvokeResponse> OnSearchInvokeAsync(ITurnContext<IInvokeActivity> turnContext, SearchInvoke searchInvoke, CancellationToken cancellationToken)
        {
            _logger!.LogInformation($"Starting application/search processing ");

            // Get session data from the invoke payload
            IEncryptionProvider encryptionProvider = _serviceProvider.GetRequiredService<IEncryptionProvider>();

            var parts = searchInvoke!.Dataset!.Split(AdaptiveDataQuery.Separator);
            var cardRoute = CardRoute.Parse(parts[0]);
            cardRoute.SessionId = await encryptionProvider.DecryptAsync(parts[1], cancellationToken);
            searchInvoke.Dataset = parts[2];
            
            var cardApp = _cardAppFactory.Create(cardRoute);
            
            await cardApp.LoadAppAsync((Activity)turnContext.Activity, cancellationToken);

            var result = await cardApp.OnSearchChoicesAsync(searchInvoke, cancellationToken);

            await cardApp.SaveAppAsync(cancellationToken);

            return result;
        }
    }
}