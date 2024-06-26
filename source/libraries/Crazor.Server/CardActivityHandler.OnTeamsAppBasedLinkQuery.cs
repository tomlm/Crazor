using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Crazor.Server
{
    public partial class CardActivityHandler
    {
        protected override Task<MessagingExtensionResponse> OnTeamsAnonymousAppBasedLinkQueryAsync(ITurnContext<IInvokeActivity> turnContext, AppBasedLinkQuery query, CancellationToken cancellationToken)
        {
            return base.OnTeamsAppBasedLinkQueryAsync(turnContext, query, cancellationToken);
        }

        /// <summary>
        /// Handle LinkQuery (aka link unfurling) request
        /// </summary>
        /// <param name="turnContext">turn context</param>
        /// <param name="query">payload</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<MessagingExtensionResponse> OnTeamsAppBasedLinkQueryAsync(ITurnContext<IInvokeActivity> turnContext, AppBasedLinkQuery query, CancellationToken cancellationToken)
        {
            var uri = new Uri(query.Url);
            System.Diagnostics.Debug.WriteLine($"Starting composeExtension/queryLink processing {uri}");
            var hostName = Context.Configuration.GetValue<Uri>("HostUri")?.Host ?? uri.Host;

            // get play page url => envir, cardId, instanceId,
            if (uri.Host == hostName)
            {
                CardRoute cardRoute = CardRoute.FromUri(uri);
                if (cardRoute == null)
                {
                    return new MessagingExtensionResponse();
                }
                var cardApp = Context.CardAppFactory.Create(cardRoute, turnContext);

                var activity = turnContext.Activity.CreateLoadRouteActivity(uri.PathAndQuery);

                await cardApp.LoadAppAsync(activity, cancellationToken);

                var card = await cardApp.ProcessInvokeActivity(activity!, isPreview: true, cancellationToken);

                // for clients that don't support AC you must send a preview card attachment.
                var preview = new Attachment(
                    contentType: "application/vnd.microsoft.card.thumbnail",
                    content: new ThumbnailCard(
                        title: "Card",
                        subtitle: "",
                        buttons: new List<CardAction>()
                        {
                            new CardAction() { Type = "openUrl", Title = "View card", Value = query.Url }
                        })
                );

                return new MessagingExtensionResponse(
                    new MessagingExtensionResult()
                    {
                        Type = "result",
                        SuggestedActions = new MessagingExtensionSuggestedAction()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(type:"setCachePolicy")
                                {
                                    Value = new JObject() { { "type", "no-cache" } }
                                }
                            }
                        },
                        AttachmentLayout = AttachmentLayoutTypes.List,
                        Attachments = new List<MessagingExtensionAttachment>()
                        {
                            new MessagingExtensionAttachment(contentType: AdaptiveCard.ContentType, content: card, preview: preview)
                        }
                    });
            }

            return await base.OnTeamsAppBasedLinkQueryAsync(turnContext, query, cancellationToken);
        }
    }
}