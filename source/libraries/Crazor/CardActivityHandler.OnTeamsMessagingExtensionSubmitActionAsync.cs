// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

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

            CardRoute cardRoute = await CardRoute.FromDataAsync(JObject.FromObject(invokeValue.Action.Data), _encryptionProvider, cancellationToken);
            var cardApp = _cardAppFactory.Create(cardRoute);
            
            cardApp.IsTaskModule = true;

            await cardApp.LoadAppAsync((Activity)turnContext.Activity, cancellationToken);

            cardApp.Action = invokeValue.Action;

            await cardApp.OnActionExecuteAsync(cancellationToken);
            
            await cardApp.SaveAppAsync(cancellationToken);

            bool isPreview = cardApp.TaskModuleAction == TaskModuleAction.Auto ||
                cardApp.TaskModuleAction == TaskModuleAction.PostCard ||
                cardApp.TaskModuleAction == TaskModuleAction.InsertCard;
            var adaptiveCard = await cardApp.RenderCardAsync(isPreview: isPreview, cancellationToken);

            await AddRefreshUserIdsAsync(turnContext, adaptiveCard, cancellationToken);

            return CreateMessagingExtensionActionResponse(action.CommandContext, cardApp, adaptiveCard);
        }

        private MessagingExtensionActionResponse CreateMessagingExtensionActionResponse(string commandContext, CardApp cardApp, AdaptiveCard adaptiveCard)
        {
            switch (cardApp.TaskModuleAction)
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
                    if (commandContext == "compose")
                    {
                        return CreateInsertCardResponse(cardApp, adaptiveCard);
                    }
                    else // if (action.CommandContext.ToLower() == "commandbox")
                    {
                        adaptiveCard.Refresh = null;
                        return CreatePreviewSendResponse(cardApp, adaptiveCard);
                    }

                case TaskModuleAction.None:
                    return null;

                default:
                    throw new Exception($"Unknown response");
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
                    Text = new Uri(_configuration.GetValue<Uri>("HostUri"), cardApp.GetCurrentCardRoute()).AbsoluteUri,
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
            AdaptiveExecuteAction? action = adaptiveCard.Refresh?.Action as AdaptiveExecuteAction;
            if (action != null)
            {
                ObjectPath.SetPathValue(action.Data, Constants.EDITSESSION_KEY, cardApp.Route.SessionId, true);
            }
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