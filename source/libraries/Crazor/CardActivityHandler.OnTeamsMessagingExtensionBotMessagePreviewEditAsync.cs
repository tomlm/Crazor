//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using AdaptiveCards;
using Crazor.Attributes;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using System.Security.Policy;

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
        protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionBotMessagePreviewEditAsync(
          ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            var card = ((JObject)action.BotActivityPreview.First().Attachments.First().Content).ToObject<AdaptiveCard>();
            var actionExecute = ((AdaptiveExecuteAction)card.Refresh.Action);
            action.Data = actionExecute.Data;
            ((JObject)action.Data)["_verb"] = actionExecute.Verb;
            return OnTeamsMessagingExtensionSubmitActionAsync(turnContext, action, cancellationToken);
        }
    }
}