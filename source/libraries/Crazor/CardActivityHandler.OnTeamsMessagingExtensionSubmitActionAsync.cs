// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
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
            System.Diagnostics.Debug.WriteLine($"Starting OnTeamsMessagingExtensionSubmitActionAsync() ");

            AdaptiveCardInvokeValue invokeValue = Utils.TransfromSubmitDataToExecuteAction(JObject.FromObject(action.Data));

            CardRoute cardRoute = await CardRoute.FromDataAsync(JObject.FromObject(invokeValue.Action.Data), Context.EncryptionProvider, cancellationToken);
            var cardApp = Context.CardAppFactory.Create(cardRoute, turnContext.TurnState.Get<IConnectorClient>());

            cardApp.IsTaskModule = true;

            await cardApp.LoadAppAsync((Activity)turnContext.Activity, cancellationToken);

            cardApp.Action = invokeValue.Action;

            await cardApp.OnActionExecuteAsync(cancellationToken);

            bool isPreview = cardApp.TaskModuleAction == TaskModuleAction.Auto ||
                cardApp.TaskModuleAction == TaskModuleAction.PostCard ||
                cardApp.TaskModuleAction == TaskModuleAction.InsertCard;

            var card = await cardApp.RenderCardAsync(isPreview: isPreview, cancellationToken);

            await cardApp.SaveAppAsync(cancellationToken);

            return CreateMessagingExtensionActionResponse(action.CommandContext, cardApp, card);
        }

        private MessagingExtensionActionResponse CreateMessagingExtensionActionResponse(string commandContext, CardApp cardApp, AdaptiveCard adaptiveCard)
        {
            switch (cardApp.TaskModuleAction)
            {
                case TaskModuleAction.Continue:
                    var submitCard = TransformCardNoRefresh(TransformActionExecuteToSubmit(adaptiveCard));
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
                    Text = new Uri(Context.Configuration.GetValue<Uri>("HostUri"), cardApp.GetCurrentCardRoute()).AbsoluteUri,
                    Attachments = new List<MessagingExtensionAttachment>()
                    {
                        // card
                        new MessagingExtensionAttachment(AdaptiveCard.ContentType, content: adaptiveCard)
                    }
                },
            };
        }

        protected static MessagingExtensionActionResponse CreatePreviewSendResponse(CardApp cardApp, AdaptiveCard card)
        {
            card = TransformActionExecuteToSubmit(card);

            AdaptiveSubmitAction refreshAction = (AdaptiveSubmitAction?)card.Refresh.Action;

            var actions = card.GetElements<AdaptiveSubmitAction>().ToList();
            foreach (var action in actions)
            {
                ObjectPath.SetPathValue(action, $"data.{Constants.EDITSESSION_KEY}", cardApp.Route.SessionId!, true);
            }
            card.Refresh = null;

            // make sure there is an action (we have to remove Refresh because teams barfs on refresh.
            if (!card.GetElements<AdaptiveSubmitAction>().Any())
            {
                if (refreshAction == null)
                {
                    refreshAction = new AdaptiveSubmitAction()
                    {
                        Data = new JObject()
                        {
                            { Constants.SUBMIT_VERB, Constants.ONEDIT_VERB },
                            { Constants.ROUTE_KEY, cardApp.Route.Route },
                            { Constants.EDITSESSION_KEY, cardApp.Route.SessionId }
                        }
                    };
                }

                card.Body.Add(new AdaptiveActionSet()
                {
                    IsVisible = false,
                    Actions = new List<AdaptiveAction>()
                    {
                        refreshAction
                    }
                });
            }

            // return preview
            return new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult(type: "botMessagePreview")
                {
                    ActivityPreview = MessageFactory.Attachment(new Attachment
                    {
                        Content = TransformCardNoRefresh(card),
                        ContentType = AdaptiveCard.ContentType
                    }) as Activity
                }
            };
        }
    }
}