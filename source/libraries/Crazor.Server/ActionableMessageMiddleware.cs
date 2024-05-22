using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace Crazor.Server
{
    /// <summary>
    /// Middleware that turns an activity with a card attachment sent to email channel into an actionable message
    /// </summary>
    public class ActionableMessageMiddleware : IMiddleware
    {
        private string _originator;

        public ActionableMessageMiddleware(IConfiguration configuration)
        {
            _originator = configuration.GetValue<string>("Channels:email:OriginatorId")!;
        }

        public Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            turnContext.OnSendActivities(async (ctx, activities, nextSend) =>
            {
                foreach (var activity in activities.Where(activity => 
                        activity.ChannelId == ChannelsEx.Email && 
                        activity.Type == ActivityTypes.Message &&
                        activity.Attachments != null &&
                        activity.Attachments.Any(attachment => attachment.ContentType == AdaptiveCard.ContentType)))
                {
                    var attachments = activity.Attachments
                        .Where(attachment => attachment.ContentType == AdaptiveCard.ContentType).ToList();

                    // remove them as we are moving card into actionable message body.
                    foreach (var attachment in attachments)
                        activity.Attachments.Remove(attachment);

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("""
<html>
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
""");
                    foreach (var card in attachments.Select(ca => ca.Content).Cast<AdaptiveCard>())
                    {
                        if (!card.AdditionalProperties.ContainsKey("originator") && _originator != null)
                        {
                            card.AdditionalProperties["originator"] = _originator;
                        }

                        if (!card.AdditionalProperties.ContainsKey("hideOriginalBody"))
                        {
                            card.AdditionalProperties["hideOriginalBody"] = true;
                        }

                        sb.AppendLine($"""
    <script type="application/adaptivecard+json">
{JsonConvert.SerializeObject(card, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented })}
    </script>
""");
                    }
                    sb.AppendLine($"""
</head>
<body>
{activity.Text}
</body>
</html>
""");
                    var channelData = activity.GetChannelData<EmailChannelData>() ?? new EmailChannelData();
                    channelData.HtmlBody = sb.ToString();
                    activity.ChannelData = channelData;
                }

                return await nextSend().ConfigureAwait(false);
            });
            return next(cancellationToken);
        }
    }

    public class EmailChannelData
    {
        [JsonProperty("importance")]
        public string? Importance { get; set; } 

        [JsonProperty("htmlBody")]
        public string? HtmlBody { get; set; }

        [JsonProperty("toRecipients")]
        public string? ToRecipients { get; set; }

        [JsonProperty("ccRecipients")]
        public string? CcRecipients { get; set; }
    }
}
