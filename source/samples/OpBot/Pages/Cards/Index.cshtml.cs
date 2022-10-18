using AdaptiveCards;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Crazor;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Neleus.DependencyInjection.Extensions;

namespace OpBot.Pages.Cards
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
            BotUri = configuration.GetValue<string>("BotUri");
        }

        public string BotUri { get; set; }

        public string? Token { get; set; }

        public CardApp? CardApp { get; set; }

        public AdaptiveCard? AdaptiveCard { get; set; }

        public async Task OnGetAsync(string app, string? resourceId, string? viewName, string? path, CancellationToken cancellationToken)
        {
            if (!app.ToLower().EndsWith("app"))
            {
                app += "App";
            }
            string appId = _configuration.GetValue<string>("MicrosoftAppId");
            string password = _configuration.GetValue<string>("MicrosoftAppPassword");
            if (appId != null && password != null)
            {
                var credentials = new MicrosoftAppCredentials(appId, password, _httpClient, null, /*oAuthScope*/appId);
                this.Token = await credentials.GetTokenAsync();
            }
            else
            {
                this.Token = String.Empty;
            }

            this.CardApp = _appFactory.GetRequiredByName(app);
            ArgumentNullException.ThrowIfNull(this.CardApp);
            string sessionId = Utils.GetNewId();

            // create card
            await this.CardApp.LoadAppAsync(resourceId, sessionId, new Activity(ActivityTypes.Invoke)
            {
                ServiceUrl = "https://about",
                ChannelId = $"emulator",
                Id = Utils.GetNewId(),
                From = new ChannelAccount() { Id = "unknown" },
                Recipient = new ChannelAccount() { Id = "bot" },
                Conversation = new ConversationAccount() { Id = resourceId ?? app },
                Value = new AdaptiveCardInvokeValue()
                {
                    Action = new AdaptiveCardInvokeAction()
                    {
                        Verb = Constants.LOADROUTE_VERB,
                        Data = new LoadRouteModel
                        {
                            View  = viewName ?? "Default",
                            Path = path
                        }
                    }
                }
            }, cancellationToken);

            // process Action.Execute
            var result = await this.CardApp.OnActionExecuteAsync(cancellationToken);

            await this.CardApp.SaveAppAsync(cancellationToken);

            this.AdaptiveCard = (AdaptiveCard)result.Value;
        }
    }
}
