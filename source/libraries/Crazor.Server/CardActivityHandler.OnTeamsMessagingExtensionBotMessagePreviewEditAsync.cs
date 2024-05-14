


using AdaptiveCards;
using Microsoft.Bot.Builder;
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
        protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionBotMessagePreviewEditAsync(
          ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            var card = ((JObject)action.BotActivityPreview.First().Attachments.First().Content).ToObject<AdaptiveCard>();
            var actionSubmit = card.GetElements<AdaptiveSubmitAction>().First();
            action.Data = actionSubmit.Data;
            ((JObject)action.Data)[Constants.SUBMIT_VERB] = Constants.EDIT_VERB;
            ((JObject)action.Data)[Constants.SESSION_KEY] = ((JObject)action.Data)[Constants.EDITSESSION_KEY];
            return OnTeamsMessagingExtensionSubmitActionAsync(turnContext, action, cancellationToken);
        }
    }
}