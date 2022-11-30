﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using Crazor.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var cardApp = context.Services.GetRequiredService<CardAppFactory>().Create(cardRoute);

            var card = await cardApp.ProcessInvokeActivity(CardTest.CreateActivity().CreateActionInvokeActivity(action.Verb, combined), false, default(CancellationToken));
            return new CardTestContext() { Services = context.Services, Card = card };
        }
   }
}