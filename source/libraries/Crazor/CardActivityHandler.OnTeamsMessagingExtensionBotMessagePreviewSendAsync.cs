// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
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
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionBotMessagePreviewSendAsync(
            ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            var activityPreview = action.BotActivityPreview[0];
            var attachmentContent = activityPreview.Attachments[0].Content;
            var previewedCard = ObjectPath.MapValueTo<AdaptiveCard>(attachmentContent);

            JObject data = new JObject();
            if (previewedCard.Refresh.Action is AdaptiveExecuteAction executeAction)
            {
                data = JObject.FromObject(executeAction.Data);
            }
            else if (previewedCard.Refresh.Action is AdaptiveSubmitAction submitAction)
            {
                data = JObject.FromObject(submitAction.Data);
            }

            var cardRoute = await CardRoute.FromDataAsync(data, _encryptionProvider, cancellationToken);

            var activity = turnContext.Activity.CreateActionInvokeActivity(Constants.SHOWVIEW_VERB);
            
            var cardApp = _cardAppFactory.Create(cardRoute);

            await cardApp.LoadAppAsync(activity, cancellationToken);
            
            await cardApp.OnActionExecuteAsync(cancellationToken);

            var adaptiveCard = await cardApp.RenderCardAsync(isPreview: true, cancellationToken);

            await AddRefreshUserIdsAsync(turnContext, adaptiveCard, cancellationToken);

            var reply = turnContext.Activity.CreateReply();
            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard
            };
            reply.Attachments.Add(attachment);

            // Attribute the message to the user on whose behalf the bot is posting
            reply.ChannelData = new
            {
                OnBehalfOf = new[]
                {
                  new
                  {
                    ItemId = 0,
                    MentionType = "person",
                    Mri = turnContext.Activity.From.Id,
                    DisplayName = turnContext.Activity.From.Name
                  }
                }
            };

            await turnContext.SendActivityAsync(reply, cancellationToken);
            return null!;
        }
    }
}