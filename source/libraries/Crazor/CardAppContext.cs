// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor.Interfaces;
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
            IEncryptionProvider encryptionProvider,
            IStorage storage)
        {
            Configuration = configuration;
            EncryptionProvider = encryptionProvider;
            Storage = storage;
            CardAppFactory = cardAppFactory;
            CardViewFactory = cardViewFactory;
            CardTabModuleFactory = cardTabModuleFactory;
            RouteResolver = routeResolver;
        }

        public IConfiguration Configuration { get; set; }

        public IEncryptionProvider EncryptionProvider { get; set; }

        public IStorage Storage { get; set; }

        public CardAppFactory CardAppFactory { get; }

        public CardTabModuleFactory CardTabModuleFactory { get; }

        public ICardViewFactory CardViewFactory { get; set; }

        public IRouteResolver RouteResolver { get; set; }
    }
}
