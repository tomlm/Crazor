//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

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
            var parts = searchInvoke!.Dataset!.Split(AdaptiveDataQuery.Separator);
            IEncryptionProvider encryptionProvider = _serviceProvider.GetRequiredService<IEncryptionProvider>();
            var data = await encryptionProvider.DecryptAsync(parts[0], cancellationToken);
            var sessionData = SessionData.FromString(data);
            searchInvoke.Dataset = parts[1];

            var cardApp = await this.LoadAppAsync(sessionData, (Activity)turnContext.Activity, cancellationToken);

            var result = await cardApp.OnSearchChoicesAsync(searchInvoke, cancellationToken);

            await cardApp.SaveAppAsync(cancellationToken);

            return result;
        }
    }
}