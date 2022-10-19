//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Crazor.Interfaces;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Neleus.DependencyInjection.Extensions;
using Newtonsoft.Json;
using System.IO;

namespace Crazor
{
    /// <summary>
    /// CardActivityHandler logic.
    /// </summary>
    public partial class CardActivityHandler : TeamsActivityHandler
    {
        private readonly IEncryptionProvider _encryptionProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CardActivityHandler>? _logger;
        private readonly IServiceByNameFactory<CardApp> _apps;
        private readonly IConfiguration _configuration;

        public CardActivityHandler(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IServiceByNameFactory<CardApp> cards,
            IEncryptionProvider encryptionProvider,
            ILogger<CardActivityHandler> logger)
        {
            ArgumentNullException.ThrowIfNull(encryptionProvider);
            ArgumentNullException.ThrowIfNull(serviceProvider);
            ArgumentNullException.ThrowIfNull(cards);
            ArgumentNullException.ThrowIfNull(configuration);
            _configuration = configuration; ;
            _encryptionProvider = encryptionProvider;
            _serviceProvider = serviceProvider;
            _apps = cards;
            _logger = logger;
        }
        protected Dictionary<string, Type> Cards { get; private set; } = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// CreateCard method for an activity context.
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="app"></param>
        /// <param name="resourceId"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public virtual async Task<CardApp> LoadAppAsync(string app, string? resourceId, string? sessionId, Activity activity, CancellationToken cancellationToken)
        {
            var cardApp = _apps.GetRequiredByName(app);
            ArgumentNullException.ThrowIfNull(cardApp);
            await cardApp.LoadAppAsync(resourceId, sessionId, activity, cancellationToken);
            return cardApp;
        }

        public virtual Task<CardApp> LoadAppAsync(SessionData sessionData, Activity activity, CancellationToken cancellationToken) =>
            LoadAppAsync(sessionData.App, sessionData.ResourceId, sessionData.SessionId, activity, cancellationToken);

        /// <summary>
        /// ShowCard - return the card in it's current state
        /// </summary>
        /// <remarks>
        /// This will resolve a Session, and so should only be called to "refresh" a card that was deliverered or unfurled.
        /// </remarks>
        /// <param name="resourceId">resourceId</param>
        /// <param name="sessionId">instanceId</param>
        /// <param name="cancellationToken">ct</param>
        /// <returns>card with session data.</returns>
        public async Task<AdaptiveCard> GetCard(ITurnContext turnContext, string app, string? resourceId, string? sessionId, string? view, CancellationToken cancellationToken)
        {
            sessionId = sessionId ?? Utils.GetNewId();

            var activity = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject((Activity)turnContext.Activity));
            var invokeValue = new AdaptiveCardInvokeValue()
            {
                Action = new AdaptiveCardInvokeAction()
                {
                    Verb = view ?? Constants.PREVIEW_VERB
                }
            };
            activity!.Value = invokeValue;
            // create card
            var cardApp = await this.LoadAppAsync(app, resourceId, sessionId, activity, cancellationToken);

            // process Action.Execute
            var result = await cardApp.OnActionExecuteAsync(cancellationToken);

            AdaptiveCard adaptiveCard = (AdaptiveCard)result.Value;
            if (turnContext.Activity.ChannelId == Channels.Msteams)
            {
                // update refresh members list.
                await SetTeamsRefreshUserIds(adaptiveCard, turnContext, cancellationToken);
            }

            await cardApp.SaveAppAsync(cancellationToken);
            return (AdaptiveCard)result.Value;
        }

        public async Task<AdaptiveCard> GetCardForUrl(ITurnContext turnContext, Uri uri, CancellationToken cancellationToken)
        {
            var parts = uri.LocalPath.Trim('/').Split('/');
            var app = parts[1] + "App";
            var resourceId = (parts.Length > 2) ? parts[2] : null;
            var view = (parts.Length > 3) ? parts[3] : null;
            var path = String.Join('/', parts.Skip(4).ToArray());
            var sessionId = turnContext?.Activity?.Id ?? Utils.GetNewId();

            var activity = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject((Activity)turnContext.Activity));
            var invokeValue = new AdaptiveCardInvokeValue()
            {
                Action = new AdaptiveCardInvokeAction()
                {
                    Verb = Constants.LOADROUTE_VERB,
                    Data = new LoadRouteModel
                    {
                        View = view ?? Constants.DEFAULT_VIEW,
                        Path = path
                    }
                }
            };
            activity!.Value = invokeValue;
            
            // create app
            var cardApp = await this.LoadAppAsync(app, resourceId, sessionId, activity, cancellationToken);

            // process Action.Execute
            var result = await cardApp.OnActionExecuteAsync(cancellationToken);

            AdaptiveCard adaptiveCard = (AdaptiveCard)result.Value;
            if (turnContext.Activity.ChannelId == Channels.Msteams)
            {
                // update refresh members list.
                await SetTeamsRefreshUserIds(adaptiveCard, turnContext, cancellationToken);
            }

            await cardApp.SaveAppAsync(cancellationToken);
            return (AdaptiveCard)result.Value;
        }

    }
}
