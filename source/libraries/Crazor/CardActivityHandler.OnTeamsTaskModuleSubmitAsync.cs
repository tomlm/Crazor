//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using AdaptiveCards;
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
        protected async override Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            _logger!.LogInformation($"Starting OnTeamsTaskModuleSubmitAsync() processing");

            dynamic data = JObject.FromObject(taskModuleRequest.Data);
            string commandId = data.commandId;
            string sessionToken = data._sessiondata;
            CardApp cardApp = null;

            Activity? showViewActivity = turnContext.Activity.CreateActionInvokeActivity(Constants.SHOWVIEW_VERB);

            SessionData sessionData = null;
            if (sessionToken != null)
            {
                AdaptiveCardInvokeValue invokeValue = Utils.TransfromSubmitDataToExecuteAction(JObject.FromObject(taskModuleRequest.Data));
                sessionData = await invokeValue.GetSessionDataFromInvokeAsync(_encryptionProvider, cancellationToken);
                cardApp = _cardAppFactory.Create(sessionData.App);
                cardApp.IsTaskModule = true;
                cardApp.Action = invokeValue.Action;
            }
            else if (commandId != null)
            {
                var uri = new Uri(_configuration.GetValue<Uri>("HostUri"), commandId);
                cardApp = _cardAppFactory.CreateFromUri(uri, out var sharedId, out var view, out var path, out var query);
                cardApp.IsTaskModule = true;
                sessionData = new SessionData() { App = cardApp.Name, SharedId = sharedId, SessionId = null};
            }
            else
                ArgumentNullException.ThrowIfNull(cardApp);

            await cardApp.LoadAppAsync(sessionData!, showViewActivity, cancellationToken);

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