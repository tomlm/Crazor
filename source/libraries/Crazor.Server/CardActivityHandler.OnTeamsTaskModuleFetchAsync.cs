﻿// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Crazor.Server
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
        protected async override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine($"Starting OnTeamsTaskModuleFetchAsync() processing");
            dynamic data = JObject.FromObject(taskModuleRequest.Data);
            string commandId = data.commandId;
            var uri = new Uri(Context.Configuration.GetValue<Uri>("HostUri"), commandId);

            CardRoute cardRoute = CardRoute.FromUri(uri);

            var cardApp = Context.CardAppFactory.Create(cardRoute, turnContext.TurnState.Get<IConnectorClient>());

            cardApp.IsTaskModule = true;

            await cardApp.LoadAppAsync((Activity)turnContext.Activity, cancellationToken);

            await cardApp.OnActionExecuteAsync(cancellationToken);

            switch (cardApp.TaskModuleAction)
            {
                case TaskModuleAction.Continue:
                    var adaptiveCard = await cardApp.RenderCardAsync(isPreview: false, cancellationToken);

                    await cardApp.SaveAppAsync(cancellationToken);

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

                default:
                    await cardApp.SaveAppAsync(cancellationToken);

                    return new TaskModuleResponse() { };
            }
        }

    }
}