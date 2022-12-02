// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;

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
            ((JObject)action.Data)["_verb"] = "OnEdit";
            ((JObject)action.Data)[Constants.SESSION_KEY] = ((JObject)action.Data)[Constants.EDITSESSION_KEY];
            return OnTeamsMessagingExtensionSubmitActionAsync(turnContext, action, cancellationToken);
        }
    }
}