// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor.Interfaces;
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
