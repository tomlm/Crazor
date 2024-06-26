using Microsoft.Bot.Builder;
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
        protected async override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionQuery query,
            CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine($"Starting OnTeamsMessagingExtensionQueryAsync() processing");

            var uri = new Uri(Context.Configuration.GetValue<Uri>("HostUri")!, query.CommandId);

            CardRoute cardRoute = CardRoute.FromUri(uri);

            var cardApp = Context.CardAppFactory.Create(cardRoute, turnContext);

            await cardApp.LoadAppAsync((Activity)turnContext.Activity, cancellationToken);

            // do the search
            var searchResults = await cardApp.OnSearchQueryAsync(query, cancellationToken);

            // don't save session data, it's a preview
            cardApp.Route.SessionId = null;

            await cardApp.SaveAppAsync(cancellationToken);

            // turn into attachments
            List<MessagingExtensionAttachment> attachments = new List<MessagingExtensionAttachment>();
            foreach (var searchResult in searchResults)
            {
                var attachment = new MessagingExtensionAttachment()
                {
                    ContentType = ThumbnailCard.ContentType,
                    Content = new ThumbnailCard()
                    {
                        Title = searchResult.Title,
                        Subtitle = searchResult.Subtitle,
                        Text = searchResult.Text,
                        Images = !String.IsNullOrEmpty(searchResult.ImageUrl) ?
                                new List<CardImage>() { new CardImage(searchResult.ImageUrl, alt: searchResult.Title) } :
                                null,
                        Tap = new CardAction()
                        {
                            Type = "invoke",
                            Value = JObject.FromObject(new { route = searchResult.Route })
                        }
                    }
                };
                attachments.Add(attachment);
            }

            return new MessagingExtensionResponse()
            {
                ComposeExtension = new MessagingExtensionResult()
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = attachments
                }
            };
        }
    }
}