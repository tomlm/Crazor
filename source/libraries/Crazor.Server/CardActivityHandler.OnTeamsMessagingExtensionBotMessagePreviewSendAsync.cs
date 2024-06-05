using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
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
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionBotMessagePreviewSendAsync(
            ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction messageAction, CancellationToken cancellationToken)
        {
            var activityPreview = messageAction.BotActivityPreview[0];
            var attachmentContent = activityPreview.Attachments[0].Content;
            var previewedCard = ObjectPath.MapValueTo<AdaptiveCard>(attachmentContent);

            // The data that comes back is missing our data, so we look for a candidate to use
            // to get our route infomration.
            JObject data = new JObject();
            var action = previewedCard.GetElements<AdaptiveAction>().Where(action => action is AdaptiveExecuteAction || action is AdaptiveSubmitAction).FirstOrDefault();
            if (action is AdaptiveExecuteAction executeAction)
            {
                data = JObject.FromObject(executeAction.Data);
            }
            else if (action is AdaptiveSubmitAction submitAction)
            {
                data = JObject.FromObject(submitAction.Data);
            }

            var cardRoute = await CardRoute.FromDataAsync(data, Context.EncryptionProvider, cancellationToken);

            var activity = turnContext.Activity.CreateLoadRouteActivity(cardRoute.Route);

            var invoke = activity.Value as AdaptiveCardInvokeValue ?? JToken.FromObject(activity.Value ?? new JObject()).ToObject<AdaptiveCardInvokeValue>();
            ArgumentNullException.ThrowIfNull(invoke);
            invoke.Action.Data = data;
            activity.Value = invoke;

            var cardApp = Context.CardAppFactory.Create(cardRoute, turnContext);

            await cardApp.LoadAppAsync(activity, cancellationToken);

            var card = await cardApp.ProcessInvokeActivity(activity, isPreview:true, cancellationToken:cancellationToken); 
            // OnActionExecuteAsync(cancellationToken);
            //var card = await cardApp.RenderCardAsync(isPreview: true, cancellationToken);

            var reply = turnContext.Activity.CreateReply();
            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
            reply.Attachments.Add(attachment);

            // Attribute the message to the user on whose behalf the bot is posting
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

            await turnContext.Adapter.SendActivitiesAsync(turnContext, new[] { reply }, cancellationToken);
            return null!;
        }
    }
}