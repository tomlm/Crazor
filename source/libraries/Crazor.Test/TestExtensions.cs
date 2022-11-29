// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using Crazor.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Crazor.Test
{
    public static class TestExtensions
    {
        /// <summary>
        /// Execute Action by id
        /// </summary>
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

        /// <summary>
        /// Assert there is a textblock[id] has text
        /// </summary>
        /// <param name="contextTask"></param>
        /// <param name="id"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static async Task<CardTestContext> AssertTextBlock(this Task<CardTestContext> contextTask, string id, string text)
        {
            var context = await contextTask;
            var actual = context.Card.GetElements<AdaptiveTextBlock>().SingleOrDefault(el => el.Id == id)?.Text;
            Assert.AreEqual(text, actual, $"TextBlock[{id}] Expected:'{text}' Actual:'{actual}'");
            return context;
        }

        /// <summary>
        /// Assert any textblock has text 
        /// </summary>
        /// <param name="contextTask"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static async Task<CardTestContext> AssertTextBlock(this Task<CardTestContext> contextTask, string text)
        {
            var context = await contextTask;
            Assert.IsTrue(context.Card.GetElements<AdaptiveTextBlock>().Any(el => el.Text == text), $"No TextBlock had expected:'{text}'");
            return context;
        }

        /// <summary>
        /// Assert card has an element of the given type and optionally id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="contextTask"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<CardTestContext> AssertElement<T>(this Task<CardTestContext> contextTask)
            where T : AdaptiveTypedElement
        {
            var context = await contextTask;
            Assert.IsTrue(context.Card.GetElements<AdaptiveTypedElement>().Any(), $"{typeof(T).Name} Not found");
            return context;
        }

        /// <summary>
        /// Assert card has an element of the given type and optionally id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="contextTask"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<CardTestContext> AssertElement<T>(this Task<CardTestContext> contextTask, string id, Action<T> callback = null)
            where T : AdaptiveTypedElement
        {
            var context = await contextTask;
            var element = (T?)context.Card.GetElements<AdaptiveTypedElement>().SingleOrDefault(el => el.Id == id);
            Assert.IsNotNull(element, $"{typeof(T).Name}[Id={id}] Not found");
            if (callback != null)
            {
                callback(element);
            }
            return context;
        }

        /// <summary>
        /// Assert card has an element of the given type and optionally id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="contextTask"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<CardTestContext> AssertMissing<T>(this Task<CardTestContext> contextTask, string id)
            where T : AdaptiveTypedElement
        {
            var context = await contextTask;
            Assert.IsFalse(context.Card.GetElements<AdaptiveTypedElement>().Any(el => el.Id == id), $"{typeof(T).Name}[Id={id}] should not be found");
            return context;
        }

        /// <summary>
        /// Assert refresh is valid
        /// </summary>
        /// <param name="contextTask"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static async Task<CardTestContext> AssertHasRefresh(this Task<CardTestContext> contextTask)
        {
            var context = await contextTask;
            Assert.IsNotNull(context.Card.Refresh);
            Assert.IsNotNull(context.Card.Refresh.Action);
            return context;
        }

        /// <summary>
        /// Assert refresh is not there
        /// </summary>
        /// <param name="contextTask"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static async Task<CardTestContext> AssertHasNoRefresh(this Task<CardTestContext> contextTask)
        {
            var context = await contextTask;
            Assert.IsNull(context.Card.Refresh);
            return context;
        }

        /// <summary>
        /// Assert card is valid
        /// </summary>
        /// <param name="contextTask"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static async Task<CardTestContext> AssertCard(this Task<CardTestContext> contextTask, Action<AdaptiveCard> callback)
        {
            var context = await contextTask;
            callback(context.Card);
            return context;
        }
    }
}