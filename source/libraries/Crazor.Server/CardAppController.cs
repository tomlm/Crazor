using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.IdentityModel.Tokens;
using System.Net;

namespace Crazor.Server.Controllers
{
    // This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    // implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    // achieved by specifying a more specific type for the bot constructor argument.
    [AllowAnonymous]
    [Route("api/cardapps")]
    [ApiController]
    public class CardAppController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter Adapter;
        private readonly IBot Bot;

        public CardAppController(IBotFrameworkHttpAdapter adapter, IBot bot)
        {
            Adapter = adapter;
            Bot = bot;
        }

        [AllowAnonymous]
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
    }
}
