using AdaptiveCards;
using Crazor.Interfaces;
using Microsoft.AspNetCore.Routing;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
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
            var activity = CardTest.CreateInvokeActivity().CreateActionInvokeActivity(action.Verb, combined);
            var cardContext = new CardTestContext()
            {
                Services = context.Services,
                Adapter = context.Adapter
            };
            await cardContext.Adapter.ProcessActivityAsync((Activity)activity, async (tc, ct) =>
            {
                cardContext.App= CardTest.Factory.Create(cardRoute, tc);
                await cardContext.App.LoadAppAsync(activity, ct);
                cardContext.Card = await cardContext.App.ProcessInvokeActivity(tc.Activity, isPreview:false, cancellationToken: ct);
            }, default);
            return cardContext;
        }
    }
}