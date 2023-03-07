// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Crazor.Server
{
    public partial class CardActivityHandler
    {
        /// <summary>
        /// OnTeamsMessagingExtensionSelectItemAsync
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject value, CancellationToken cancellationToken)
        {
            string route = (string)value["route"]!;
            var uri = new Uri(Context.Configuration.GetValue<Uri>("HostUri"), route);

            CardRoute cardRoute = CardRoute.FromUri(uri);

            var cardApp = Context.CardAppFactory.Create(cardRoute, turnContext);

            var activity = turnContext.Activity.CreateLoadRouteActivity(cardRoute.Route);

            var card = await cardApp.ProcessInvokeActivity(activity, isPreview: true, cancellationToken);

            var preview = new Attachment(
                contentType: "application/vnd.microsoft.card.thumbnail",
                content: new ThumbnailCard(
                    title: "Card",
                    subtitle: "",
                    buttons: new List<CardAction>()
                    {
                        new CardAction() { Type = "openUrl", Title = "View card", Value = uri.AbsoluteUri }
                    })
            );

            return new MessagingExtensionResponse(
                new MessagingExtensionResult()
                {
                    Type = "result",
                    AttachmentLayout = AttachmentLayoutTypes.List,
                    Attachments = new List<MessagingExtensionAttachment>()
                    {
                        new MessagingExtensionAttachment(contentType: AdaptiveCard.ContentType, content: card, preview: preview)
                    }
                });
        }
    }
}