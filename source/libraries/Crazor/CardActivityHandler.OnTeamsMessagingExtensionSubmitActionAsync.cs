//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            CancellationToken cancellationToken)
        {
            _logger!.LogInformation($"Starting OnTeamsMessagingExtensionSubmitActionAsync() ");

            AdaptiveCardInvokeValue invokeValue = Utils.TransfromSubmitDataToExecuteAction(JObject.FromObject(action.Data));

            SessionData sessionData = await invokeValue.GetSessionDataFromInvokeAsync(_encryptionProvider, cancellationToken);
            var cardApp = await this.LoadAppAsync(sessionData, (Activity)turnContext.Activity, cancellationToken);

            cardApp.IsTaskModule = true;
            cardApp.MessageExtensionAction = action;
            cardApp.Action = invokeValue.Action;

            var adaptiveCard = await cardApp.OnActionExecuteAsync(cancellationToken);
            await cardApp.SaveAppAsync(cancellationToken);

            switch (cardApp.TaskModuleStatus)
            {
                case TaskModuleAction.Continue:
                    adaptiveCard.Refresh = null;
                    var submitCard = TransformActionExecuteToSubmit(adaptiveCard);
                    // continue taskModule bound to current card view.
                    return new MessagingExtensionActionResponse()
                    {
                        Task = new TaskModuleContinueResponse()
                        {
                            Value = GetTaskInfoForCard(cardApp, submitCard)
                        },
                    };

                case TaskModuleAction.InsertCard:
                    return CreateInsertCardResponse(cardApp, adaptiveCard);

                case TaskModuleAction.PostCard:
                    return CreatePreviewSendResponse(cardApp, adaptiveCard);

                case TaskModuleAction.Auto:
                    if (action.CommandContext == "compose")
                    {
                        return CreateInsertCardResponse(cardApp, adaptiveCard);
                    }
                    else // if (action.CommandContext.ToLower() == "commandbox")
                    {
                        return CreatePreviewSendResponse(cardApp, adaptiveCard);
                    }

                case TaskModuleAction.None:
                default:
                    return new MessagingExtensionActionResponse() { };
            }
        }


        protected MessagingExtensionActionResponse CreateInsertCardResponse(CardApp cardApp, AdaptiveCard adaptiveCard)
        {
            // insert card into output
            return new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult(attachmentLayout: "list", type: "result")
                {
                    // url to card
                    Text = new Uri(_configuration.GetValue<Uri>("HostUri"), cardApp.GetRoute()).AbsoluteUri,
                    Attachments = new List<MessagingExtensionAttachment>()
                    {
                        // card
                        new MessagingExtensionAttachment(AdaptiveCard.ContentType, content: adaptiveCard)
                    }
                },
            };
        }

        protected static MessagingExtensionActionResponse CreatePreviewSendResponse(CardApp cardApp, AdaptiveCard adaptiveCard)
        {
            // return preview
            return new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult(type: "botMessagePreview")
                {
                    ActivityPreview = MessageFactory.Attachment(new Attachment
                    {
                        Content = adaptiveCard,
                        ContentType = AdaptiveCard.ContentType
                    }) as Activity
                }
            };
        }
    }
}