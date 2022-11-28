using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using System.Threading;
using Microsoft.IdentityModel.Tokens;
using System.Net;

namespace Crazor.Controllers
{
    // This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    // implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    // achieved by specifying a more specific type for the bot constructor argument.
    [Route("api/cardapps")]
    [ApiController]
    public class CardAppController : ControllerBase
    {
        private static HttpClient _httpClient = new HttpClient();

        private readonly IBotFrameworkHttpAdapter Adapter;
        private readonly IBot Bot;

        public CardAppController(IBotFrameworkHttpAdapter adapter, IBot bot)
        {
            Adapter = adapter;
            Bot = bot;
        }

        [HttpPost]
        public async Task PostAsync()
        {
            try
            {
                // Delegate the processing of the HTTP POST to the adapter.
                // The adapter will invoke the bot.
                await Adapter.ProcessAsync(Request, Response, Bot);
            }
            catch (SecurityTokenExpiredException err)
            {
                System.Diagnostics.Trace.TraceError(err.Message);
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
        }

        public static async Task<string> GetTokenAsync(IConfiguration configuration)
        {
            string appId = configuration.GetValue<string>("MicrosoftAppId");
            if (appId != null)
            {
                var credentialsFactory = new ConfigurationServiceClientCredentialFactory(configuration);
                var credentials = (AppCredentials)await credentialsFactory.CreateCredentialsAsync(appId, appId, AuthenticationConstants.ToChannelFromBotLoginUrlTemplate, false, CancellationToken.None);
                return await credentials.GetTokenAsync();
            }
            else
            {
                return String.Empty;
            }
        }
    }
}
