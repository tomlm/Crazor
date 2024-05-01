


using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;

namespace Crazor.Server
{
    // Create the Bot Adapter with error handling enabled.
    public class AdapterWithErrorHandler : CloudAdapter
    {
        public AdapterWithErrorHandler(BotFrameworkAuthentication auth, ILogger<IBotFrameworkHttpAdapter> logger, IStorage storage)
            : base(auth, logger)
        {
            
            this.Use(new SSOTokenExchangeMiddleware(storage));

            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                // NOTE: In production environment, you should consider logging this to
                // Azure Application Insights. Visit https://aka.ms/bottelemetry to see how
                // to add telemetry capture to your bot.
#pragma warning disable CA2254 // Template should be a static expression
                logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");
#pragma warning restore CA2254 // Template should be a static expression

                // Note: Since this Messaging Extension does not have the messageTeamMembers permission
                // in the manifest, the bot will not be allowed to message users.
                // await turnContext.SendActivityAsync("The bot encountered an error or bug.");
                // await turnContext.SendActivityAsync("To continue to run this bot, please fix the bot source code.");

                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
            };
        }
    }
}
