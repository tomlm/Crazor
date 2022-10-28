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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;

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
            _logger!.LogInformation($"Starting OnTeamsMessagingExtensionFetchTaskAsync() processing");

            var uri = new Uri(_configuration.GetValue<Uri>("HostUri"), action.CommandId);

            var cardApp = await LoadAppAsync(turnContext, uri, cancellationToken);
            cardApp.IsTaskModule = true;

            var adaptiveCard = await cardApp.OnActionExecuteAsync(cancellationToken);
            await cardApp.SaveAppAsync(cancellationToken);

            var submitCard = TransformActionExecuteToSubmit(adaptiveCard);

            return new MessagingExtensionActionResponse()
            {
                Task = new TaskModuleContinueResponse()
                {
                    Value = GetTaskInfoForCard(cardApp, submitCard)
                },
            };
        }

        protected AdaptiveCard TransformActionExecuteToSubmit(AdaptiveCard card)
        {
            foreach(var action in card.GetElements<AdaptiveExecuteAction>())
            {
                if (action.Data == null)
                {
                    action.Data = new JObject();
                }
                ((JObject)action.Data)[AdaptiveExecuteAction.TypeName] = JObject.FromObject(action);
            }
            var json = JsonConvert.SerializeObject(card);
            json = json.Replace(AdaptiveExecuteAction.TypeName, AdaptiveSubmitAction.TypeName);
            return JsonConvert.DeserializeObject<AdaptiveCard>(json)!;
        }

        protected TaskModuleTaskInfo GetTaskInfoForCard(CardApp cardApp, AdaptiveCard adaptiveCard)
        {
            var taskModuleAttribute = cardApp.CurrentView.GetType().GetCustomAttribute<TaskModuleAttribute>();
            var taskInfo = taskModuleAttribute?.AsTaskInfo(cardApp.Name) ??
                new TaskModuleTaskInfo()
                {
                    Title = cardApp.Name,
                    Height = "small",
                    Width = "small"
                };
            taskInfo.FallbackUrl = new Uri(_configuration.GetValue<Uri>("HostUri"), cardApp.GetRoute()).AbsoluteUri;
            taskInfo.Url = _configuration.GetValue<string>("BotUri") ?? new Uri(_configuration.GetValue<Uri>("HostUri"), "/api/cardapps").AbsoluteUri; 
            taskInfo.CompletionBotId = _configuration.GetValue<string>("MicrosoftAppId");
            taskInfo.Card = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard
            };
            return taskInfo;
        }
    }
}