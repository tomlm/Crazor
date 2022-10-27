using AdaptiveCards;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Crazor;
using Microsoft.Bot.Schema;
using Neleus.DependencyInjection.Extensions;
using Crazor.Controllers;

namespace PhysOpBot.Pages.Cards
{
    public class CardHostModel : PageModel
    {
        private IServiceByNameFactory<CardApp> _appFactory;
        private IConfiguration _configuration;

        public CardHostModel(IConfiguration configuration, IServiceByNameFactory<CardApp> cardFactory)
        {
            _configuration = configuration;
            _appFactory = cardFactory;
            BotUri = configuration.GetValue<string>("BotUri") ?? new Uri(configuration.GetValue<Uri>("HostUri"), "/api/cardapps").AbsoluteUri;
        }

        public string BotUri { get; set; }

        public string? Token { get; set; }

        public CardApp? CardApp { get; set; }

        public AdaptiveCard? AdaptiveCard { get; set; }

        public async Task OnGetAsync(string app, string? sharedId, string? viewName, string? path, CancellationToken cancellationToken)
        {
            if (!app.ToLower().EndsWith("app"))
            {
                app += "App";
            }

            this.Token = await CardAppController.GetTokenAsync(_configuration);

            this.CardApp = _appFactory.GetRequiredByName(app);
            ArgumentNullException.ThrowIfNull(this.CardApp);
            string sessionId = Utils.GetNewId();

            // create card
            await this.CardApp.LoadAppAsync(sharedId, sessionId, new Activity(ActivityTypes.Invoke)
            {
                ServiceUrl = "https://about",
                ChannelId = $"emulator",
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
                            View = viewName ?? "Default",
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
