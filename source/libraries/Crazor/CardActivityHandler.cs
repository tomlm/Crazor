//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Crazor.Interfaces;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Neleus.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using AdaptiveCards;
using Microsoft.Bot.Connector;
using System.Threading;

namespace Crazor
{
    /// <summary>
    /// CardActivityHandler logic.
    /// </summary>
    public partial class CardActivityHandler : TeamsActivityHandler
    {
        protected readonly IEncryptionProvider _encryptionProvider;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly ILogger<CardActivityHandler>? _logger;
        protected readonly CardAppFactory _cardAppFactory;
        protected readonly IServiceByNameFactory<CardTabModule> _tabs;
        protected readonly IConfiguration _configuration;
        
        private static JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
        {
            Formatting = Newtonsoft.Json.Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
        };

        public CardActivityHandler(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            _serviceProvider = serviceProvider;
            _configuration = _serviceProvider.GetRequiredService<IConfiguration>();
            _encryptionProvider = _serviceProvider.GetService<IEncryptionProvider>()!;
            _cardAppFactory = _serviceProvider.GetRequiredService<CardAppFactory>();
            _tabs = _serviceProvider.GetRequiredService<IServiceByNameFactory<CardTabModule>>();
            _logger = _serviceProvider.GetService<ILogger<CardActivityHandler>>();
        }

        public async Task AddRefreshUserIdsAsync(ITurnContext turnContext, AdaptiveCard card, CancellationToken cancellationToken)
        {
            if (card.Refresh != null)
            {
                if (turnContext.Activity.ChannelId == Channels.Msteams && !turnContext.Activity.Conversation.Id.StartsWith("tab:"))
                {
                    // we need to add refresh userids
                    var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
                    var teamsMembers = await connectorClient.Conversations.GetConversationPagedMembersAsync(turnContext.Activity.Conversation.Id, 60, cancellationToken: cancellationToken);
                    card.Refresh.UserIds = teamsMembers.Members.Select(member => $"8:orgid:{member.AadObjectId}").ToList();
                }
            }
        }
    }
}
