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
        protected readonly IEncryptionProvider _encryptionProvider;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly ILogger<CardActivityHandler>? _logger;
        protected readonly IServiceByNameFactory<CardApp> _apps;
        protected readonly IConfiguration _configuration;

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
        /// <param name="sharedId"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public virtual async Task<CardApp> LoadAppAsync(string app, string? sharedId, string? sessionId, Activity activity, CancellationToken cancellationToken)
        {
            var cardApp = _apps.GetRequiredByName(app);
            ArgumentNullException.ThrowIfNull(cardApp);
            await cardApp.LoadAppAsync(sharedId, sessionId, activity, cancellationToken);
            return cardApp;
        }

        public virtual Task<CardApp> LoadAppAsync(SessionData sessionData, Activity activity, CancellationToken cancellationToken) =>
            LoadAppAsync(sessionData.App, sessionData.SharedId, sessionData.SessionId, activity, cancellationToken);

        /// <summary>
        /// ShowCard - return the card in it's current state
        /// </summary>
        /// <remarks>
        /// This will resolve a Session, and so should only be called to "refresh" a card that was deliverered or unfurled.
        /// </remarks>
        /// <param name="sharedId">sharedId</param>
        /// <param name="sessionId">instanceId</param>
        /// <param name="cancellationToken">ct</param>
        /// <returns>card with session data.</returns>
        public async Task<CardApp> LoadAppAsync(ITurnContext turnContext, string app, string? sharedId, string? sessionId, string? view, CancellationToken cancellationToken)
        {
            sessionId = sessionId ?? Utils.GetNewId();

            var activity = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject((Activity)turnContext.Activity));
            var invokeValue = new AdaptiveCardInvokeValue()
            {
                Action = new AdaptiveCardInvokeAction()
                {
                    Verb = view ?? Constants.REFRESH_VERB
                }
            };
            activity!.Value = invokeValue;
            // create card
            return await this.LoadAppAsync(app, sharedId, sessionId, activity, cancellationToken);
        }

        public async Task<CardApp> LoadAppAsync(ITurnContext turnContext, Uri uri, CancellationToken cancellationToken)
        {
            ParsePath(uri.LocalPath, out var app, out var sharedId, out var view, out var path);

            var sessionId = turnContext?.Activity?.Id ?? Utils.GetNewId();

            var activity = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject((Activity)turnContext?.Activity!));
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

            return await this.LoadAppAsync(app, sharedId, sessionId, activity, cancellationToken);
        }

        /// <summary>
        /// Parse a /cards/{app}/{view}{path} into parts.
        /// </summary>
        /// <param name="localPath"></param>
        /// <param name="app"></param>
        /// <param name="sharedId"></param>
        /// <param name="view"></param>
        /// <param name="path"></param>
        protected static void ParsePath(string localPath, out string app, out string? sharedId, out string? view, out string path)
        {
            var parts = localPath.Trim('/').Split('/');
            app = parts[1] + "App";
            sharedId = (parts.Length > 2) ? parts[2] : null;
            view = (parts.Length > 3) ? parts[3] : null;
            path = String.Join('/', parts.Skip(4).ToArray());
        }
    }
}
