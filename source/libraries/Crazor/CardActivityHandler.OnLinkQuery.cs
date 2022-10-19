//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Crazor
{
    public partial class CardActivityHandler
    {
        /// <summary>
        /// Handle LinkQuery (aka link unfurling) request
        /// </summary>
        /// <param name="turnContext">turn context</param>
        /// <param name="query">payload</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<MessagingExtensionResponse> OnTeamsAppBasedLinkQueryAsync(ITurnContext<IInvokeActivity> turnContext, AppBasedLinkQuery query, CancellationToken cancellationToken)
        {
            var uri = new Uri(query.Url);
            _logger!.LogInformation($"Starting composeExtension/queryLink processing {uri}");
            var hostName = _configuration.GetValue<string>("HostName") ?? uri.Host;

            // get play page url => envir, cardId, instanceId,
            if (uri.Host == hostName)
            {
                var adaptiveCard = await GetCardForUrl(turnContext, uri, cancellationToken);

                // for clients that don't support AC you must send a preview card attachment.
                var preview = new Attachment(
                    contentType: "application/vnd.microsoft.card.thumbnail",
                    content: new ThumbnailCard(
                        title: "Card",
                        subtitle: "",
                        buttons: new List<CardAction>()
                        {
                                new CardAction() { Type = "openUrl", Title = "View card", Value = query.Url }
                        })
                );

                return new MessagingExtensionResponse(
                    new MessagingExtensionResult()
                    {
                        Type = "result",
                        AttachmentLayout = AttachmentLayoutTypes.List,
                        Attachments = new List<MessagingExtensionAttachment>()
                        {
                            new MessagingExtensionAttachment(contentType: AdaptiveCard.ContentType, content: adaptiveCard, preview: preview)
                        }
                    });
            }

            return await base.OnTeamsAppBasedLinkQueryAsync(turnContext, query, cancellationToken);
        }
    }
}