using AdaptiveCards;
using Crazor.Interfaces;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;

namespace Crazor
{
    /// <summary>
    /// Implement this class to create a tab made up of cards in your project
    /// </summary>
    public abstract class CardTabModule
    {
        protected IServiceProvider _serviceProvider;
        protected IConfiguration _configuration;
        protected CardAppFactory _cardAppFactory;
        protected IEncryptionProvider _encryptionProvider;
        protected ILogger? _logger;
        protected IStorage _storage;

        public CardTabModule(IServiceProvider services, string? name = null)
        {
            _serviceProvider = services;
            _configuration = services.GetRequiredService<IConfiguration>();
            _cardAppFactory = services.GetRequiredService<CardAppFactory>();
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
            string tabSessionId = turnContext.Activity.Conversation.Id;
            var key = GetKey(tabSessionId);
            var result = await _storage.ReadAsync(new string[] { key }, cancellationToken);
            CardTabModuleState tabState = result.ContainsKey(key) ? result[key] as CardTabModuleState ?? new CardTabModuleState() : new CardTabModuleState();

            foreach (var cardUri in cardUris)
            {
                // if we have a refresh action cached for this uri
                if (tabState.RefreshMap.TryGetValue(cardUri, out var refreshAction))
                {
                    // we use it to do a refresh
                    var cardRoute = await CardRoute.FromDataAsync(JObject.FromObject(refreshAction.Data), _encryptionProvider, cancellationToken);

                    taskCards.Add(InvokeTabCardAsync(turnContext!, cardRoute, refreshAction.CreateInvokeValue(turnContext), cancellationToken));
                }
                else
                {
                    if (Uri.TryCreate(cardUri, UriKind.RelativeOrAbsolute, out var uri))
                    {
                        // we do a load route on the uri
                        uri = uri.IsAbsoluteUri ? uri : new Uri(_configuration.GetValue<Uri>("HostUri"), uri);
                        taskCards.Add(LoadTabCardAsync(turnContext!, uri, tabSessionId, cancellationToken));
                    }
                }
            }

            await Task.WhenAll(taskCards);

            var cards = taskCards.Select(task => task.Result).ToArray();
            for (int i = 0; i < cardUris.Length; i++)
            {
                tabState.RefreshMap[cardUris[i]] = (AdaptiveExecuteAction)cards[i].Refresh.Action;
            }

            await _storage.WriteAsync(new Dictionary<string, object>() { { GetKey(tabSessionId), tabState } }, cancellationToken);

            return cards;
        }

        protected string GetKey(string tabSessionId) => $"{this.Name}.{tabSessionId}";

        public virtual async Task<AdaptiveCard[]> OnTabSubmitAsync(ITurnContext turnContext, TabSubmit tabSubmit, AdaptiveCardInvokeValue invokeValue, CancellationToken cancellationToken)
        {
            CardRoute cardRoute = await CardRoute.FromDataAsync(JObject.FromObject(invokeValue.Action.Data), _encryptionProvider, cancellationToken);

            var cardUris = await GetCardUrisAsync();

            string tabSessionId = turnContext.Activity.Conversation.Id;
            var key = GetKey(tabSessionId);
            var result = await _storage.ReadAsync(new string[] { key }, cancellationToken);

            CardTabModuleState tabState = (CardTabModuleState)result[key];

            List<Task<AdaptiveCard>> taskCards = new List<Task<AdaptiveCard>>();
            foreach (string cardUri in cardUris)
            {
                if (tabState.RefreshMap.TryGetValue(cardUri, out var refreshAction))
                {
                    CardRoute cardRouteData = await CardRoute.FromDataAsync(JObject.FromObject(refreshAction.Data), _encryptionProvider, cancellationToken);

                    if (cardRouteData.App == cardRoute.App)
                    {
                        // this is the actual button click
                        taskCards.Add(InvokeTabCardAsync(turnContext!, cardRoute, invokeValue, cancellationToken));
                    }
                    else
                    {
                        // use the refresh action.
                        taskCards.Add(InvokeTabCardAsync(turnContext!, cardRouteData, refreshAction.CreateInvokeValue(turnContext), cancellationToken));
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
            ArgumentNullException.ThrowIfNull(turnContext);
            ArgumentNullException.ThrowIfNull(uri);
            ArgumentNullException.ThrowIfNull(cancellationToken);

            var cardApp = _cardAppFactory.Create(CardRoute.FromUri(uri));

            var card = await cardApp.ProcessInvokeActivity(turnContext.Activity.CreateLoadRouteActivity(uri.PathAndQuery), false, cancellationToken);
            
            return card;
        }

        protected async Task<AdaptiveCard> InvokeTabCardAsync(ITurnContext turnContext, CardRoute cardRoute, AdaptiveCardInvokeValue invokeValue, CancellationToken cancellationToken)
        {
            var cardApp = _cardAppFactory.Create(cardRoute);
            var card = await cardApp.ProcessInvokeActivity(turnContext.Activity.CreateActionInvokeActivity(invokeValue.Action.Verb, JObject.FromObject(invokeValue.Action.Data)), false, cancellationToken);
            return card;
        }
    }
}
