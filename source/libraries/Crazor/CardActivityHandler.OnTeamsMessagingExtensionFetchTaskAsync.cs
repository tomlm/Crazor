//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using AdaptiveCards;
using Crazor.Attributes;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Security.Policy;

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
        protected async override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            _logger!.LogInformation($"Starting composeExtension/fetchTask processing");
            string appId = action.CommandId;

            var cardApp = await LoadAppAsync(turnContext, appId, resourceId: null, sessionId: null, view: null, cancellationToken: cancellationToken);
            var invokeResponse = await cardApp.OnActionExecuteAsync(cancellationToken);
            await cardApp.SaveAppAsync(cancellationToken);
            var adaptiveCard = (AdaptiveCard)invokeResponse.Value;

            // get task module data for taskInfo
            var taskModuleAttribute = cardApp.CurrentView.GetType().GetCustomAttribute<TaskModuleAttribute>();
            var taskInfo = taskModuleAttribute?.AsTaskInfo(cardApp.Name)  ?? 
                new TaskModuleTaskInfo()
                {
                    Title = cardApp.Name,
                    Height = "small",
                    Width = "small"
                };
            taskInfo.FallbackUrl = cardApp.CurrentView.GetRoute();
            taskInfo.Url = _configuration.GetValue<string>("BotUri") ?? new Uri(_configuration.GetValue<Uri>("HostUri"), "/api/cardapps").AbsoluteUri;
            taskInfo.CompletionBotId = _configuration.GetValue<string>("MicrosoftAppId");
            taskInfo.Card = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard
            };

            return new MessagingExtensionActionResponse()
            {
                Task = new TaskModuleContinueResponse()
                {
                    Value = taskInfo
                },
            };
        }
    }
}