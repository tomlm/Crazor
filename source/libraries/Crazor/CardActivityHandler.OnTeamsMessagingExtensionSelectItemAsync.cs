﻿//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using AdaptiveCards;
using Crazor.Attributes;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using static System.Collections.Specialized.BitVector32;

namespace Crazor
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
            var uri = new Uri(_configuration.GetValue<Uri>("HostUri"), route);

            CardRoute cardRoute = CardRoute.FromUri(uri);

            var cardApp = _cardAppFactory.Create(cardRoute);

            var activity = turnContext.Activity.CreateLoadRouteActivity(cardRoute.Route);

            var card = await cardApp.ProcessInvokeActivity(activity, isPreview: true, cancellationToken);

            await AddRefreshUserIdsAsync(turnContext, card, cancellationToken);

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