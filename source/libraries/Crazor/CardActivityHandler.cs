﻿//-----------------------------------------------------------------------------
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
using Newtonsoft.Json.Linq;

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
            _encryptionProvider = _serviceProvider.GetService<IEncryptionProvider>();
            _cardAppFactory = _serviceProvider.GetRequiredService<CardAppFactory>();
            _tabs = _serviceProvider.GetRequiredService<IServiceByNameFactory<CardTabModule>>();
            _logger = _serviceProvider.GetService<ILogger<CardActivityHandler>>();
        }

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
            var cardApp = _cardAppFactory.Create(app);
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
        public async Task<CardApp> LoadAppAsync(Activity sourceActivity, string app, string? sharedId, string? sessionId, string? view, CancellationToken cancellationToken)
        {
            sessionId = sessionId ?? Utils.GetNewId();

            Activity? activity = sourceActivity.CreateActionInvokeActivity(view);

            // create card
            return await this.LoadAppAsync(app, sharedId, sessionId, activity, cancellationToken);
        }

        public async Task<CardApp> LoadAppAsync(Activity sourceActivity, Uri uri, CancellationToken cancellationToken)
        {
            CardApp.ParseUri(uri, out var app, out var sharedId, out var view, out var path);
            dynamic value = JObject.FromObject(sourceActivity.Value);
            string verb = (string)value?.action?.verb!;
            var loadRouteActivity = sourceActivity;
            if (verb != Constants.LOADROUTE_VERB)
            {
                loadRouteActivity = sourceActivity.CreateLoadRouteActivity(view, path);
            }

            var sessionId = sourceActivity?.Id ?? Utils.GetNewId();
            return await this.LoadAppAsync(app, sharedId, sessionId, loadRouteActivity, cancellationToken);
        }
    }
}
