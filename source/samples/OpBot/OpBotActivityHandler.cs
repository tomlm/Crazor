using AdaptiveCards;
using Crazor;
using Crazor.Attributes;
using Crazor.Interfaces;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Neleus.DependencyInjection.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Crazor.OpBotActivityHandler
{
    public class OpBotActivityHandlerApp : CardActivityHandler
    {
        public OpBotActivityHandlerApp(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IServiceByNameFactory<CardApp> cards,
            IEncryptionProvider encryptionProvider,
            ILogger<CardActivityHandler> logger)
            : base(serviceProvider, configuration, cards, encryptionProvider, logger)
        {
        }

    }
}
