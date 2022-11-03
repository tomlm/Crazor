using AdaptiveCards;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Crazor;
using Microsoft.Bot.Schema;
using Neleus.DependencyInjection.Extensions;
using Crazor.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace PhysOpBot.Pages.Cards
{
    public class CardHostModel : PageModel
    {
        private static HttpClient _httpClient = new HttpClient();
        private IServiceByNameFactory<CardApp> _appFactory;
        private IConfiguration _configuration;

        public CardHostModel(IConfiguration configuration, IServiceByNameFactory<CardApp> cardFactory)
        {
            _configuration = configuration;
            _appFactory = cardFactory;
            BotUri = configuration.GetValue<string>("BotUri") ?? new Uri(configuration.GetValue<Uri>("HostUri"), "/api/cardapps").AbsoluteUri;
            ChannelId = _configuration.GetValue<Uri>("HostUri").Host;
        }

        public string BotUri { get; set; }

        public string ChannelId { get; set; }

        public string? Token { get; set; }

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

            this.Token = await CardAppController.GetTokenAsync(_configuration);

            this.CardApp = _appFactory.GetRequiredByName(app);
            ArgumentNullException.ThrowIfNull(this.CardApp);

            // create card
            await this.CardApp.LoadAppAsync(sharedId: sharedId, sessionId: sessionId, new Activity(ActivityTypes.Invoke)
            {
                ServiceUrl = "https://about",
                ChannelId = this.ChannelId,
                Id = Utils.GetNewId(),
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
                            View = viewName ?? Constants.DEFAULT_VIEW,
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
