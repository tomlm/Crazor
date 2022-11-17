//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

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
            var previewedCard = ((JObject)attachmentContent).ToObject<AdaptiveCard>();

            // Get session data from the invoke payload
            dynamic data = ((AdaptiveExecuteAction)previewedCard!.Refresh.Action).Data;
            var sd = (string)data._sessiondata;
            sd = await _encryptionProvider.DecryptAsync(sd, cancellationToken);
            var sessionData = SessionData.FromString(sd);
            var activity = turnContext.Activity.CreateActionInvokeActivity(Constants.SHOWVIEW_VERB);
            var cardApp = await this.LoadAppAsync(sessionData, activity, cancellationToken);
            
            await cardApp.OnActionExecuteAsync(cancellationToken);

            var adaptiveCard = await cardApp.RenderCardAsync(isPreview: true, cancellationToken);

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

            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
            await connectorClient.Conversations.SendToConversationAsync(reply, cancellationToken);
            return null!;
        }
    }
}