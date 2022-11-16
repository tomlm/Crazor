using AdaptiveCards;
using Crazor;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using System;
using Microsoft.AspNetCore.Mvc.Routing;

namespace CrazorDemoBot.Cards.Cards
{
    public class CardsApp : CardApp
    {
        public CardsApp(IServiceProvider services, CardAppFactory cardFactory)
            : base(services)
        {
            CardFactory = cardFactory;
        }

        public CardAppFactory CardFactory { get; }

        /// <summary>
        /// lower level method to handle search command
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<MessagingExtensionResponse> OnMessagingExtensionQueryAsync(MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            // do the search
            var searchTerm = query.Parameters.SingleOrDefault(p => p.Name == "search")?.Value.ToString() ?? String.Empty;
            var names = CardFactory.GetNames().Where(name => name.ToLower().Contains(searchTerm.ToLower()));

            // turn into attachments
            List<MessagingExtensionAttachment> attachments = new List<MessagingExtensionAttachment>();
            foreach (var appName in names)
            {
                // replaceview with new view
                var loadRouteActivity = Activity!.CreateLoadRouteActivity(null!,null!);

                var cardApp = CardFactory.Create(appName);

                await cardApp.LoadAppAsync(null, null, loadRouteActivity, cancellationToken);

                await cardApp.OnActionExecuteAsync(cancellationToken);

                // await cardApp.SaveAppAsync(cancellationToken);

                var card = await cardApp.RenderCardAsync(isPreview: true, cancellationToken);

                var attachment = new MessagingExtensionAttachment()
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = card,
                    Preview = new MessagingExtensionAttachment()
                    {
                        ContentType = ThumbnailCard.ContentType,
                        Content = new ThumbnailCard()
                        {
                            Title = appName,
                            Subtitle = card.Title,
                            Images = new List<CardImage>() { new CardImage(new Uri(this.GetCurrentCardUri(), "/images/card.png").AbsoluteUri) }
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
