using AdaptiveCards;
using Crazor.Attributes;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Crazor.Server
{
    public partial class CardActivityHandler
    {
        /// <summary>
        /// Handle Fetch Task request
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="messageExtensionAction"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction messageExtensionAction, CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine($"Starting OnTeamsMessagingExtensionFetchTaskAsync() processing");

            var loadRouteActivity = turnContext.Activity.CreateLoadRouteActivity(messageExtensionAction.CommandId);
            var uri = new Uri(Context.Configuration.GetValue<Uri>("HostUri"), messageExtensionAction.CommandId);
            CardRoute cardRoute = CardRoute.FromUri(uri);

            var cardApp = Context.CardAppFactory.Create(cardRoute, turnContext);

            cardApp.CommandContext = messageExtensionAction.CommandContext;
            cardApp.IsTaskModule = true;

            await cardApp.LoadAppAsync(loadRouteActivity, cancellationToken);

            var card = await cardApp.ProcessInvokeActivity(loadRouteActivity, isPreview: false, cancellationToken);

            return CreateMessagingExtensionActionResponse(messageExtensionAction.CommandContext, cardApp, card);
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
                ((JObject)action.Data)[Constants.SUBMIT_VERB] = action.Verb;
                action.Verb = null;
            }
            // no refresh if only submit
            card.Refresh = null;

            var json = JsonConvert.SerializeObject(card, _jsonSettings);
            json = json.Replace($"\"type\": \"{AdaptiveExecuteAction.TypeName}\"", $"\"type\": \"{AdaptiveSubmitAction.TypeName}\"");
            return JsonConvert.DeserializeObject<AdaptiveCard>(json, _jsonSettings)!;
        }

        protected TaskModuleTaskInfo GetTaskInfoForCard(CardApp cardApp, AdaptiveCard adaptiveCard)
        {
            var viewTaskInfo = cardApp.CurrentView.GetType().GetCustomAttribute<TaskInfoAttribute>();
            var appTaskInfo = cardApp.GetType().GetCustomAttribute<TaskInfoAttribute>();

            var taskInfo = new TaskModuleTaskInfo()
            {
                Title = viewTaskInfo?.Title ?? appTaskInfo?.Title ?? adaptiveCard?.Title ?? cardApp.Name,
                Width = viewTaskInfo?.Width ?? appTaskInfo?.Width ?? "medium",
                Height = viewTaskInfo?.Height ?? appTaskInfo?.Height ?? "medium",
            };

            taskInfo.FallbackUrl = new Uri(Context.Configuration.GetValue<Uri>("HostUri"), cardApp.GetCurrentCardRoute()).AbsoluteUri;
            // taskInfo.Url = Context.Configuration.GetValue<string>("BotUri") ?? new Uri(Context.Configuration.GetValue<Uri>("HostUri"), "/api/cardapps").AbsoluteUri; 
            taskInfo.CompletionBotId = Context.Configuration.GetValue<string>("MicrosoftAppId");
            taskInfo.Card = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard
            };
            return taskInfo;
        }
    }
}