using AdaptiveCards;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Bot.Schema;
using Crazor.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http.Extensions;

namespace Crazor.HostPage.Pages.Cards
{
    public class CardHostModel : PageModel
    {
        private static HttpClient _httpClient = new HttpClient();
        private CardAppFactory _cardAppFactory;
        private IConfiguration _configuration;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CardHostModel(IConfiguration configuration, CardAppFactory cardAppFactory)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _configuration = configuration;
            _cardAppFactory = cardAppFactory;
            BotUri = configuration.GetValue<string>("BotUri") ?? new Uri(configuration.GetValue<Uri>("HostUri"), "/api/cardapps").AbsoluteUri;
            ChannelId = _configuration.GetValue<Uri>("HostUri").Host;
        }

        public string BotUri { get; set; }

        public string ChannelId { get; set; }

        public CardApp? CardApp { get; set; }

        public AdaptiveCard? AdaptiveCard { get; set; }

        public string RouteUrl { get; set; }

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            string userId = null;
            if (this.Request.Cookies.TryGetValue("userId", out var uid))
            {
                userId = uid;
            }
            else
            {
                userId = Utils.GetNewId();
            }

            var uri = new Uri(Request.GetDisplayUrl());
            this.CardApp = _cardAppFactory.CreateFromUri(uri, out var sharedId, out var view, out var path, out var query);
            ArgumentNullException.ThrowIfNull(this.CardApp);

            string sessionId = Utils.GetNewId();
            var loadRouteActivity = new Activity(ActivityTypes.Invoke)
            {
                ServiceUrl = "https://about",
                ChannelId = this.ChannelId,
                Id = Guid.NewGuid().ToString("n"),
                From = new ChannelAccount() { Id = userId },
                Recipient = new ChannelAccount() { Id = "bot" },
                Conversation = new ConversationAccount() { Id = sharedId },
                Timestamp = DateTimeOffset.UtcNow,
                LocalTimestamp = DateTimeOffset.Now,
            }
            .CreateLoadRouteActivity(uri);

            await this.CardApp.LoadAppAsync(sharedId, sessionId, loadRouteActivity, cancellationToken);

            var token = await CardAppController.GetTokenAsync(_configuration);
            this.Response.Cookies.Append("token", token);
            this.Response.Cookies.Append("userId", userId);
            this.Response.Cookies.Append("sharedId", CardApp.SharedId ?? String.Empty);

            // process Action.Execute
            await this.CardApp.OnActionExecuteAsync(cancellationToken);

            await this.CardApp.SaveAppAsync(cancellationToken);

            this.AdaptiveCard = await this.CardApp.RenderCardAsync(isPreview: false, cancellationToken);

            this.RouteUrl = this.CardApp.GetCurrentCardRoute();
        }
    }
}
