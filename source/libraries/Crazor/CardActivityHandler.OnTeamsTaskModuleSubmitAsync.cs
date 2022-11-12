//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using AdaptiveCards;
using Crazor.Attributes;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector;
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

            if (sessionToken != null)
            {
                AdaptiveCardInvokeValue invokeValue = Utils.TransfromSubmitDataToExecuteAction(JObject.FromObject(taskModuleRequest.Data));
                SessionData sessionData = await invokeValue.GetSessionDataFromInvokeAsync(_encryptionProvider, cancellationToken);
                cardApp = await this.LoadAppAsync(sessionData, showViewActivity, cancellationToken);
                cardApp.IsTaskModule = true;
                cardApp.Action = invokeValue.Action;
            }
            else if (commandId != null)
            {
                var uri = new Uri(_configuration.GetValue<Uri>("HostUri"), commandId);
                cardApp = await LoadAppAsync(showViewActivity, uri, cancellationToken);
                cardApp.IsTaskModule = true;
            }
            else
                ArgumentNullException.ThrowIfNull(cardApp);

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