
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Crazor
{
    /// <summary>
    /// Implement this class to create a tab made up of cards in your project
    /// </summary>
    public abstract class CardTabModule
    {

        public CardTabModule(CardAppContext context, string? name = null)
        {
            Context = context;
            Name = name ?? this.GetType().Name;
        }

        public CardAppContext Context { get; }

        /// <summary>
        /// Tab Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Implement this to return the card paths you want to display to the user in this tab.
        /// </summary>
        /// <returns></returns>
        public abstract Task<string[]> GetCardUrisAsync();

        public static IEnumerable<TypeInfo> GetTabModuleTypes()
        {
            return Utils.GetAssemblies().SelectMany(asm => asm.DefinedTypes.Where(t => t.IsAssignableTo(typeof(CardTabModule)) && t.IsAbstract == false));
        }

        public virtual async Task<AdaptiveCard[]> OnTabFetchAsync(ITurnContext turnContext, TabRequest tabRequest, CancellationToken cancellationToken)
        {
            List<Task<AdaptiveCard>> taskCards = new List<Task<AdaptiveCard>>();
            var cardUris = await GetCardUrisAsync();
            string tabSessionId = turnContext.Activity.Conversation.Id;
            var key = GetKey(tabSessionId);
            var result = await Context.Storage.ReadAsync(new string[] { key }, cancellationToken);
            CardTabModuleState tabState = result.ContainsKey(key) ? result[key] as CardTabModuleState ?? new CardTabModuleState() : new CardTabModuleState();

            // NOTE: THIS CODE WILL NOT WORK IF THERE IS MORE THAN ONE CARD
            // THIS SHOULD BE REWRITTEN.
            foreach (var cardUri in cardUris.Take(1))
            {
                // if we have a refresh action cached for this uri
                if (tabState.RefreshMap.TryGetValue(cardUri, out var refreshAction))
                {
                    // we use it to do a refresh
                    var cardRoute = await CardRoute.FromDataAsync(JObject.FromObject(refreshAction.Data), Context.EncryptionProvider, cancellationToken);

                    taskCards.Add(InvokeTabCardAsync(turnContext!, cardRoute, refreshAction.CreateInvokeValue(turnContext), cancellationToken));
                }
                else
                {
                    if (Uri.TryCreate(cardUri, UriKind.RelativeOrAbsolute, out var uri))
                    {
                        // we do a load route on the uri
                        uri = uri.IsAbsoluteUri ? uri : new Uri(Context.Configuration.GetValue<Uri>("HostUri"), uri);
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

            await Context.Storage.WriteAsync(new Dictionary<string, object>() { { GetKey(tabSessionId), JObject.FromObject(tabState) } }, cancellationToken);

            return cards;
        }

        protected string GetKey(string tabSessionId) => $"{this.Name}.{tabSessionId}";

        public virtual async Task<AdaptiveCard[]> OnTabSubmitAsync(ITurnContext turnContext, TabSubmit tabSubmit, AdaptiveCardInvokeValue invokeValue, CancellationToken cancellationToken)
        {
            CardRoute cardRoute = await CardRoute.FromDataAsync(JObject.FromObject(invokeValue.Action.Data), Context.EncryptionProvider, cancellationToken);

            var cardUris = await GetCardUrisAsync();

            string tabSessionId = turnContext.Activity.Conversation.Id;
            var key = GetKey(tabSessionId);
            var result = await Context.Storage.ReadAsync(new string[] { key }, cancellationToken);

            CardTabModuleState tabState = ((JObject)result[key]).ToObject<CardTabModuleState>()!;

            List<Task<AdaptiveCard>> taskCards = new List<Task<AdaptiveCard>>();
            foreach (string cardUri in cardUris)
            {
                if (tabState.RefreshMap.TryGetValue(cardUri, out var refreshAction))
                {
                    CardRoute cardRouteData = await CardRoute.FromDataAsync(JObject.FromObject(refreshAction.Data), Context.EncryptionProvider, cancellationToken);

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
                        uri = uri.IsAbsoluteUri ? uri : new Uri(Context.Configuration.GetValue<Uri>("HostUri"), uri);
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

            var cardApp = Context.CardAppFactory.Create(CardRoute.FromUri(uri), turnContext);

            var activity = turnContext.Activity.CreateLoadRouteActivity(uri.PathAndQuery);

            await cardApp.LoadAppAsync(activity, cancellationToken);

            var card = await cardApp.ProcessInvokeActivity(activity, false, cancellationToken);

            return card;
        }

        protected async Task<AdaptiveCard> InvokeTabCardAsync(ITurnContext turnContext, CardRoute cardRoute, AdaptiveCardInvokeValue invokeValue, CancellationToken cancellationToken)
        {
            var cardApp = Context.CardAppFactory.Create(cardRoute, turnContext);
            var activity = turnContext.Activity.CreateActionInvokeActivity(invokeValue.Action.Verb, JObject.FromObject(invokeValue.Action.Data));
            await cardApp.LoadAppAsync(activity, cancellationToken);
            var card = await cardApp.ProcessInvokeActivity(activity, false, cancellationToken);
            return card;
        }
    }
}
