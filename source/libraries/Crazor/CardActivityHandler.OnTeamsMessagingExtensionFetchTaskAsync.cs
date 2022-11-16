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

            var activity = turnContext.Activity.CreateActionInvokeActivity(Constants.SHOWVIEW_VERB);
            var cardApp = await LoadAppAsync(activity, uri, cancellationToken);
            cardApp.IsTaskModule = true;

            await cardApp.OnActionExecuteAsync(cancellationToken);
            
            await cardApp.SaveAppAsync(cancellationToken);

            var adaptiveCard = await cardApp.RenderCardAsync(isPreview: false, cancellationToken);

            return CreateMessagingExtensionActionResponse(action.CommandContext, cardApp, adaptiveCard);
        }

        protected static AdaptiveCard TransformCardNoRefresh(AdaptiveCard card)
        {
            card.Refresh = null;
            return card;
        }

        protected static AdaptiveCard TransformActionExecuteToSubmit(AdaptiveCard card)
        {
            foreach (var action in card.GetElements<AdaptiveExecuteAction>())
            {
                if (action.Data == null)
                {
                    action.Data = new JObject();
                }
                ((JObject)action.Data)["_verb"] = action.Verb;
                action.Verb = null;
            }
            var json = JsonConvert.SerializeObject(card, _jsonSettings);
            json = json.Replace($"\"type\": \"{AdaptiveExecuteAction.TypeName}\"", $"\"type\": \"{AdaptiveSubmitAction.TypeName}\"");
            return JsonConvert.DeserializeObject<AdaptiveCard>(json, _jsonSettings)!;
        }

        protected TaskModuleTaskInfo GetTaskInfoForCard(CardApp cardApp, AdaptiveCard adaptiveCard)
        {
            var taskModuleAttribute = cardApp.CurrentView.GetType().GetCustomAttribute<TaskModuleAttribute>();
            var taskInfo = taskModuleAttribute?.AsTaskInfo(adaptiveCard?.Title ?? cardApp.Name) ??
                new TaskModuleTaskInfo()
                {
                    Title = adaptiveCard?.Title ?? cardApp.Name,
                    Height = "medium",
                    Width = "medium"
                };
            taskInfo.FallbackUrl = new Uri(_configuration.GetValue<Uri>("HostUri"), cardApp.GetCurrentCardRoute()).AbsoluteUri;
            // taskInfo.Url = _configuration.GetValue<string>("BotUri") ?? new Uri(_configuration.GetValue<Uri>("HostUri"), "/api/cardapps").AbsoluteUri; 
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