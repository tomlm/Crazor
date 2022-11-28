// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

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
        protected async override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            _logger!.LogInformation($"Starting OnTeamsTaskModuleFetchAsync() processing");
            dynamic data = JObject.FromObject(taskModuleRequest.Data);
            string commandId = data.commandId;
            var uri = new Uri(_configuration.GetValue<Uri>("HostUri"), commandId);

            CardRoute cardRoute = CardRoute.FromUri(uri);

            var cardApp = _cardAppFactory.Create(cardRoute);

            cardApp.IsTaskModule = true;

            await cardApp.LoadAppAsync((Activity)turnContext.Activity, cancellationToken);
                        
            await cardApp.OnActionExecuteAsync(cancellationToken);
            
            await cardApp.SaveAppAsync(cancellationToken);

            switch (cardApp.TaskModuleAction)
            {
                case TaskModuleAction.Continue:
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

                default:
                    return new TaskModuleResponse() { };
            }
        }

    }
}