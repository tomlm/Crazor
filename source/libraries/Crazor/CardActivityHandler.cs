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
            _encryptionProvider = _serviceProvider.GetService<IEncryptionProvider>();
            _apps = _serviceProvider.GetRequiredService<IServiceByNameFactory<CardApp>>();
            _tabs = _serviceProvider.GetRequiredService<IServiceByNameFactory<CardTabModule>>();
            _logger = _serviceProvider.GetService<ILogger<CardActivityHandler>>();
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

            Activity? activity = turnContext.CreateActionInvokeActivity(view);

            // create card
            return await this.LoadAppAsync(app, sharedId, sessionId, activity, cancellationToken);
        }

        public async Task<CardApp> LoadAppAsync(ITurnContext turnContext, Uri uri, CancellationToken cancellationToken)
        {
            CardApp.ParseUri(uri, out var app, out var sharedId, out var view, out var path);
            var loadRouteActivity = turnContext.CreateLoadRouteActivity(view, path);

            var sessionId = turnContext?.Activity?.Id ?? Utils.GetNewId();

            return await this.LoadAppAsync(app, sharedId, sessionId, loadRouteActivity, cancellationToken);
        }
    }
}
