// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Bot.Builder;
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
            CardTabModuleFactory cardTabModuleFactory,
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
            CardTabModuleFactory = cardTabModuleFactory;
            RouteResolver = routeResolver;
            ServiceOptions = options;
            AuthorizationService = authorizationService;
            AuthenticationStateProvider = authenticationStateProvider;
            User = new ClaimsPrincipal(new ClaimsIdentity()); ;
        }

        public IServiceProvider ServiceProvider { get; set; }

        public IConfiguration Configuration { get; set; }

        public IEncryptionProvider EncryptionProvider { get; set; }

        public IStorage Storage { get; set; }

        public CardAppFactory CardAppFactory { get; }

        public CardTabModuleFactory CardTabModuleFactory { get; }

        public IRouteResolver RouteResolver { get; set; }

        public ServiceOptions ServiceOptions { get; }

        public IAuthorizationService AuthorizationService { get; }

        public AuthenticationStateProvider AuthenticationStateProvider { get; }

        /// <summary>
        /// User principal
        /// </summary>
        public ClaimsPrincipal User { get; set; }

        /// <summary>
        /// UserToken from SSO token exchange
        /// </summary>
        public TokenResponse? UserToken { get; set; }
    }
}
