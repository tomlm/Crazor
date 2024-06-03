using AdaptiveCards;
using Crazor.Interfaces;
using Crazor.Teams;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;
using System.Security.Claims;

namespace Crazor
{
    public class CardAppContext
    {
        public CardAppContext(
            IServiceProvider servicesProvider,
            CardAppFactory cardAppFactory,
            IRouteResolver routeResolver,
            IConfiguration configuration,
            IEncryptionProvider encryptionProvider,
            IStorage storage,
            IAuthorizationService authorizationService,
            AuthenticationStateProvider authenticationStateProvider,
            ServiceOptions options)
        {
            ServiceProvider = servicesProvider;
            Configuration = configuration;
            EncryptionProvider = encryptionProvider;
            Storage = storage;
            CardAppFactory = cardAppFactory;
            RouteResolver = routeResolver;
            ServiceOptions = options;
            AuthorizationService = authorizationService;
            AuthenticationStateProvider = authenticationStateProvider;
            User = new ClaimsPrincipal(new ClaimsIdentity());
        }

        public IServiceProvider ServiceProvider { get; set; }

        public IConfiguration Configuration { get; set; }

        public IEncryptionProvider EncryptionProvider { get; set; }

        public IStorage Storage { get; set; }

        public CardAppFactory CardAppFactory { get; }

        public IRouteResolver RouteResolver { get; set; }

        public ServiceOptions ServiceOptions { get; }

        public IAuthorizationService AuthorizationService { get; }
        
        public AuthenticationStateProvider AuthenticationStateProvider { get; }

        public CardApp App { get; set; }

        /// <summary>
        /// User principal
        /// </summary>
        public ClaimsPrincipal User { get; set; }

        public ITurnContext TurnContext { get; set; }

        /// GetPreviewCardForRoute()
        /// </summary>
        /// <param name="route">route to get preview card for</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>adaptive card suitable for sharing</returns>
        public async Task<AdaptiveCard> GetPreviewCardForRoute(string route, ITurnContext context, CancellationToken cancellationToken)
        {
            CardRoute cardRoute = CardRoute.Parse(route);
            var cardApp = CardAppFactory.Create(cardRoute, context);
            var activity = context.Activity.CreateLoadRouteActivity(cardRoute.Route);
            await cardApp.LoadAppAsync(activity, cancellationToken);
            var card = await cardApp.ProcessInvokeActivity(activity!, isPreview: true, cancellationToken);
            return card;
        }
    }
}
