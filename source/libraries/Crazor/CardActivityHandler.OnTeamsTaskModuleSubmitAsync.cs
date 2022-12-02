// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
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
        protected async override Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            _logger!.LogInformation($"Starting OnTeamsTaskModuleSubmitAsync() processing");

            JObject data = JObject.FromObject(taskModuleRequest.Data);
            data[Constants.ROUTE_KEY] = (string)data["commandId"];
            CardRoute cardRoute = await CardRoute.FromDataAsync(data, _encryptionProvider, cancellationToken);

            var showViewActivity = turnContext.Activity.CreateActionInvokeActivity(Constants.SHOWVIEW_VERB);

            AdaptiveCardInvokeValue invokeValue = Utils.TransfromSubmitDataToExecuteAction(JObject.FromObject(taskModuleRequest.Data));
            
            var cardApp = _cardAppFactory.Create(cardRoute);
            cardApp.IsTaskModule = true;
            cardApp.Action = invokeValue.Action;

            await cardApp.LoadAppAsync(showViewActivity, cancellationToken);

            await cardApp.OnActionExecuteAsync(cancellationToken);

            await cardApp.SaveAppAsync(cancellationToken);

            switch (cardApp.TaskModuleAction)
            {
                case TaskModuleAction.Continue:
                    {
                        var adaptiveCard = await cardApp.RenderCardAsync(isPreview: false, cancellationToken);

                        adaptiveCard.Refresh = null;
                        var submitCard = TransformActionExecuteToSubmit(adaptiveCard);
                        // continue taskModule bound to current card view.
                        return new TaskModuleResponse()
                        {
                            Task = new TaskModuleContinueResponse()
                            {
                                Value = GetTaskInfoForCard(cardApp, submitCard)
                            },
                        };
                    }

                case TaskModuleAction.Auto:
                case TaskModuleAction.PostCard:
                    {
                        var adaptiveCard = await cardApp.RenderCardAsync(isPreview: true, cancellationToken);

                        await AddRefreshUserIdsAsync(turnContext, adaptiveCard, cancellationToken);

                        var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
                        var reply = turnContext.Activity.CreateReply();
                        cardApp.IsTaskModule = false;
                        reply.Attachments.Add(new Attachment()
                        {
                            ContentType = AdaptiveCard.ContentType,
                            Content = adaptiveCard
                        });
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

                        await connectorClient.Conversations.SendToConversationAsync(reply, cancellationToken);
                    }
                    return null!;
                case TaskModuleAction.InsertCard:
                case TaskModuleAction.None:
                default:
                    return null!;
            }
        }

    }
}