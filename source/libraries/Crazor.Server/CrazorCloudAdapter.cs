using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Crazor.Server
{
    // Create the Bot Adapter with error handling enabled.
    public class CrazorCloudAdapter : CloudAdapter
    {
        private readonly ConnectorFactory _connectorFactory;
        private readonly ClaimsIdentity _claimsIdentity;
        private readonly string _botAppId;

        public CrazorCloudAdapter(BotFrameworkAuthentication auth, ILogger<IBotFrameworkHttpAdapter> logger, IStorage storage, IConfiguration configuration)
            : base(auth, logger)
        {

            this.Use(new SSOTokenExchangeMiddleware(storage, configuration));
            this.Use(new ActionableMessageMiddleware(configuration));

            _botAppId = configuration.GetValue<String>("MicrosoftAppId")!;

            _claimsIdentity = new ClaimsIdentity(new List<Claim>
            {
                // Adding claims for both Emulator and Channel.
                new Claim(AuthenticationConstants.AudienceClaim, _botAppId),
                new Claim(AuthenticationConstants.AppIdClaim, _botAppId),
            });

            _connectorFactory = this.BotFrameworkAuthentication.CreateConnectorFactory(_claimsIdentity);

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

        /// <summary>
        /// Process an activity that has already been authenticated externally (for example because of secure blazor connection)
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="callback"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<InvokeResponse> ProcessAuthenticatedActivityAsync(Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            AuthenticateRequestResult authenticateRequestResult = new AuthenticateRequestResult()
            {
                Audience = _botAppId,
                CallerId = null,
                ClaimsIdentity = _claimsIdentity,
                ConnectorFactory = _connectorFactory,
            };

            return this.ProcessActivityAsync(authenticateRequestResult, activity, callback, cancellationToken);
        }

    }
}
