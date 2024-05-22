using Crazor.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
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
            UserTokenClient userTokenClient,
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
            User = new ClaimsPrincipal(new ClaimsIdentity()); ;
            UserTokenClient = userTokenClient;
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

        /// <summary>
        /// UserToken from SSO token exchange
        /// </summary>
        public UserTokenClient UserTokenClient { get; set; }

        public Dictionary<string, TokenResponse> TokenResponses { get; set; } = new Dictionary<string, TokenResponse>();
    }
}
