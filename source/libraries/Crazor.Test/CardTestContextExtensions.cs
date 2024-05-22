using AdaptiveCards;
using Crazor.Interfaces;
using Newtonsoft.Json.Linq;

namespace Crazor.Test
{
    public static class CardTestContextExtensions
    {
        /// <param name="contextTask"></param>
        /// <param name="idOrVerb">id of the action</param>
        /// <param name="data">optional data to merge in to simulate input controls</param>
        /// <returns></returns>
        public static async Task<CardTestContext> ExecuteAction(this Task<CardTestContext> contextTask, string idOrVerb, object data = null)
        {
            var context = await contextTask;

            var action = context.Card.GetElements<AdaptiveExecuteAction>().SingleOrDefault(a => a.Id == idOrVerb) ??
                context.Card.GetElements<AdaptiveExecuteAction>().First(a => a.Verb == idOrVerb);

            var combined = (JObject)JObject.FromObject(action.Data).DeepClone();
            if (data != null)
                combined.Merge(JObject.FromObject(data));

            CardRoute cardRoute = await CardRoute.FromDataAsync(combined, context.Services.GetRequiredService<IEncryptionProvider>(), default(CancellationToken));
            var cardApp = context.Services.GetRequiredService<CardAppFactory>().Create(cardRoute, null);

            var activity = CardTest.CreateInvokeActivity().CreateActionInvokeActivity(action.Verb, combined);

            await cardApp.LoadAppAsync(activity, default);

            var card = await cardApp.ProcessInvokeActivity(activity, false, default(CancellationToken));

            return new CardTestContext() { Services = context.Services, Card = card };
        }
    }
}