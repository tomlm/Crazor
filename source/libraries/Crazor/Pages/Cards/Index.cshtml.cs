using AdaptiveCards;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Crazor;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Neleus.DependencyInjection.Extensions;
using Crazor.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Crazor.HostPage.Pages.Cards
{
    public class CardHostModel : PageModel
    {
        private static HttpClient _httpClient = new HttpClient();
        private IServiceByNameFactory<CardApp> _appFactory;
        private IConfiguration _configuration;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CardHostModel(IConfiguration configuration, IServiceByNameFactory<CardApp> cardFactory)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _configuration = configuration;
            _appFactory = cardFactory;
            BotUri = configuration.GetValue<string>("BotUri") ?? new Uri(configuration.GetValue<Uri>("HostUri"), "/api/cardapps").AbsoluteUri;
            ChannelId = _configuration.GetValue<Uri>("HostUri").Host;
        }

        public string BotUri { get; set; }

        public string ChannelId { get; set; }

        public CardApp? CardApp { get; set; }

        public AdaptiveCard? AdaptiveCard { get; set; }

        public string RouteUrl { get; set; }

        public async Task OnGetAsync(string app, [FromQuery(Name = "id")] string? sharedId, string? viewName, string? path, CancellationToken cancellationToken)
        {
            if (!app.ToLower().EndsWith("app"))
            {
                app += "App";
            }

            var sessionId = Utils.GetNewId();

            var token = await CardAppController.GetTokenAsync(_configuration);
            this.Response.Cookies.Append("token", token);

            this.CardApp = _appFactory.GetRequiredByName(app);
            ArgumentNullException.ThrowIfNull(this.CardApp);

            // create card
            await this.CardApp.LoadAppAsync(sharedId: sharedId, sessionId: sessionId, new Activity(ActivityTypes.Invoke)
            {
                ServiceUrl = "https://about",
                ChannelId = this.ChannelId,
                Id = Guid.NewGuid().ToString("n"),
                From = new ChannelAccount() { Id = "unknown" },
                Recipient = new ChannelAccount() { Id = "bot" },
                Conversation = new ConversationAccount() { Id = sharedId ?? app },
                Value = new AdaptiveCardInvokeValue()
                {
                    Action = new AdaptiveCardInvokeAction()
                    {
                        Verb = Constants.LOADROUTE_VERB,
                        Data = new LoadRouteModel
                        {
                            View  = viewName ?? Constants.DEFAULT_VIEW,
                            Path = path
                        }
                    }
                }
            }, cancellationToken);

            // process Action.Execute
            this.AdaptiveCard = await this.CardApp.OnActionExecuteAsync(cancellationToken);

            await this.CardApp.SaveAppAsync(cancellationToken);

            this.RouteUrl = this.CardApp.GetRoute();
        }
    }
}
