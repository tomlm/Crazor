using AdaptiveCards;
using Crazor.Interfaces;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neleus.DependencyInjection.Extensions;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Bot.Connector;

namespace Crazor
{
    /// <summary>
    /// Implement this class to create a tab made up of cards in your project
    /// </summary>
    public abstract class CardTabModule
    {
        protected IServiceProvider _serviceProvider;
        protected IConfiguration _configuration;
        protected IServiceByNameFactory<CardApp> _cardApps;
        protected IEncryptionProvider _encryptionProvider;
        protected ILogger? _logger;
        protected IStorage _storage;

        public CardTabModule(IServiceProvider services, string? name = null)
        {
            _serviceProvider = services;
            _configuration = services.GetRequiredService<IConfiguration>();
            _cardApps = services.GetRequiredService<IServiceByNameFactory<CardApp>>();
            _encryptionProvider = services.GetRequiredService<IEncryptionProvider>();
            _logger = services.GetService<ILogger>();
            _storage = services.GetRequiredService<IStorage>();
            Name = name ?? this.GetType().Name;
        }

        /// <summary>
        /// Tab Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Implement this to return the card paths you want to display to the user in this tab.
        /// </summary>
        /// <returns></returns>
        public abstract Task<string[]> GetCardUrisAsync();

        public virtual async Task<AdaptiveCard[]> OnTabFetchAsync(ITurnContext turnContext, TabRequest tabRequest, CancellationToken cancellationToken)
        {
            List<Task<AdaptiveCard>> taskCards = new List<Task<AdaptiveCard>>();
            var cardUris = await GetCardUrisAsync();
            var sessionId = turnContext.Activity.Conversation.Id;
            var key = GetKey(sessionId);
            var result = await _storage.ReadAsync(new string[] { key }, cancellationToken);
            CardTabModuleState tabState = result.ContainsKey(key) ? result[key] as CardTabModuleState ?? new CardTabModuleState() : new CardTabModuleState();

            foreach (var cardUri in cardUris)
            {
                // if we have a sessiondata for this uri
                if (tabState.RefreshMap.TryGetValue(cardUri, out var refreshAction))
                {
                    // we do a refresh
                    var sessionData = await refreshAction.GetSessionDataFromActionAsync(_encryptionProvider, cancellationToken);
                    taskCards.Add(InvokeTabCardAsync(turnContext!, sessionData, refreshAction.CreateInvokeValue(turnContext), cancellationToken));
                }
                else
                {
                    if (Uri.TryCreate(cardUri, UriKind.RelativeOrAbsolute, out var uri))
                    {
                        // we do a load route
                        uri = uri.IsAbsoluteUri ? uri : new Uri(_configuration.GetValue<Uri>("HostUri"), uri);
                        taskCards.Add(LoadTabCardAsync(turnContext!, uri, sessionId, cancellationToken));
                    }
                }
            }

            await Task.WhenAll(taskCards);

            var cards = taskCards.Select(task => task.Result).ToArray();
            for (int i = 0; i < cardUris.Length; i++)
            {
                tabState.RefreshMap[cardUris[i]] = (AdaptiveExecuteAction)cards[i].Refresh.Action;
            }

            await _storage.WriteAsync(new Dictionary<string, object>() { { GetKey(sessionId), tabState } }, cancellationToken);

            return cards;
        }

        protected string GetKey(string sessionId) => $"{this.Name}.{sessionId}";

        public virtual async Task<AdaptiveCard[]> OnTabSubmitAsync(ITurnContext turnContext, TabSubmit tabSubmit, AdaptiveCardInvokeValue invokeValue, CancellationToken cancellationToken)
        {
            SessionData invokeSessionData = await invokeValue.GetSessionDataFromInvokeAsync(_encryptionProvider, cancellationToken);
            var cardUris = await GetCardUrisAsync();

            var key = GetKey(invokeSessionData.SessionId!);
            var result = await _storage.ReadAsync(new string[] { key }, cancellationToken);

            CardTabModuleState tabState = (CardTabModuleState)result[key];

            List<Task<AdaptiveCard>> taskCards = new List<Task<AdaptiveCard>>();
            foreach (string cardUri in cardUris)
            {
                if (tabState.RefreshMap.TryGetValue(cardUri, out var refreshAction))
                {
                    var sessionData = await refreshAction.GetSessionDataFromActionAsync(_encryptionProvider, cancellationToken);

                    if (sessionData.App == invokeSessionData.App)
                    {
                        // this is the actual button click
                        taskCards.Add(InvokeTabCardAsync(turnContext!, sessionData, invokeValue, cancellationToken));
                    }
                    else
                    {
                        // this is the refresh action.
                        taskCards.Add(InvokeTabCardAsync(turnContext!, sessionData, refreshAction.CreateInvokeValue(turnContext), cancellationToken));
                    }
                }
                else
                {
                    if (Uri.TryCreate(cardUri, UriKind.RelativeOrAbsolute, out var uri))
                    {
                        // we do a load route
                        var sessionId = turnContext.Activity.Conversation.Id;
                        uri = uri.IsAbsoluteUri ? uri : new Uri(_configuration.GetValue<Uri>("HostUri"), uri);
                        taskCards.Add(LoadTabCardAsync(turnContext!, uri, sessionId, cancellationToken));
                    }
                }

            }

            await Task.WhenAll(taskCards);

            return taskCards.Select(task => task.Result).ToArray();
        }

        protected async Task<AdaptiveCard> LoadTabCardAsync(ITurnContext turnContext, Uri uri, string sessionId, CancellationToken cancellationToken)
        {
            CardApp.ParseUri(uri, out var app, out var sharedId, out var view, out var path);

            var loadRouteActivity = turnContext.Activity.CreateLoadRouteActivity(view, path);

            var cardApp = _cardApps.GetRequiredByName(app);

            await cardApp.LoadAppAsync(sharedId, sessionId, loadRouteActivity, cancellationToken);

            await cardApp.OnActionExecuteAsync(cancellationToken);

            await cardApp.SaveAppAsync(cancellationToken);

            var card = await cardApp.RenderCardAsync(isPreview: false, cancellationToken);

            if (turnContext.Activity.ChannelId == Channels.Msteams &&
    !turnContext.Activity.Conversation.Id.StartsWith("tab:"))
            {
                // we need to add refresh userids
                var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
                var teamsMembers = await connectorClient.Conversations.GetConversationPagedMembersAsync(turnContext.Activity.Conversation.Id, 60, cancellationToken: cancellationToken);
                card.Refresh.UserIds = teamsMembers.Members.Select(member => $"8:orgid:{member.AadObjectId}").ToList();
            }

            return card;
        }

        protected async Task<AdaptiveCard> InvokeTabCardAsync(ITurnContext turnContext, SessionData sessionData, AdaptiveCardInvokeValue invokeValue, CancellationToken cancellationToken)
        {
            var cardApp = _cardApps.GetRequiredByName(sessionData.App);

            await cardApp.LoadAppAsync(sessionData.SharedId, sessionData.SessionId, (Activity)turnContext.Activity, cancellationToken);
            cardApp.Action = invokeValue.Action;
            
            await cardApp.OnActionExecuteAsync(cancellationToken);
            
            await cardApp.SaveAppAsync(cancellationToken);
            
            var adaptiveCard = await cardApp.RenderCardAsync(isPreview: false, cancellationToken);
            return adaptiveCard;
        }
    }
}
