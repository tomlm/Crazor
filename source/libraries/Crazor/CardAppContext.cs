// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;

namespace Crazor
{
    public class CardAppContext
    {
        public CardAppContext(
            CardAppFactory cardAppFactory,
            CardTabModuleFactory cardTabModuleFactory,
            ICardViewFactory cardViewFactory,
            IRouteResolver routeResolver,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IEncryptionProvider encryptionProvider,
            IStorage storage)
        {
            Configuration = configuration;
            HttpContextAccessor = httpContextAccessor;
            EncryptionProvider = encryptionProvider;
            Storage = storage;
            CardAppFactory = cardAppFactory;
            CardViewFactory = cardViewFactory;
            CardTabModuleFactory = cardTabModuleFactory;
            RouteResolver = routeResolver;
        }

        public IConfiguration Configuration { get; set; }

        public IHttpContextAccessor HttpContextAccessor { get; }

        public IEncryptionProvider EncryptionProvider { get; set; }

        public IStorage Storage { get; set; }

        public CardAppFactory CardAppFactory { get; }

        public CardTabModuleFactory CardTabModuleFactory { get; }

        public ICardViewFactory CardViewFactory { get; set; }

        public IRouteResolver RouteResolver { get; set; }
    }
}
