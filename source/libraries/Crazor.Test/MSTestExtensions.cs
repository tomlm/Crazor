// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Crazor.Test.MSTest
{
    public static class MSTestExtensions
    {
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
            Assert.IsTrue(context.Card.GetElements<AdaptiveTextBlock>().Any(el => el.Text == text), $"TextBlock couldn't be found with:'{text}'");
            return context;
        }

        /// <summary>
        /// Assert text block is not there
        /// </summary>
        /// <param name="contextTask"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static async Task<CardTestContext> AssertNoTextBlock(this Task<CardTestContext> contextTask, string text)
        {
            var context = await contextTask;
            Assert.IsFalse(context.Card.GetElements<AdaptiveTextBlock>().Any(el => el.Text == text), $"TextBlock found with:'{text}'");
            return context;
        }

        /// <summary>
        /// Assert card has an element of the given type and optionally id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="contextTask"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<CardTestContext> AssertElements<T>(this Task<CardTestContext> contextTask, Action<IEnumerable<T>> callback)
            where T : AdaptiveTypedElement
        {
            var context = await contextTask;
            callback(context.Card.GetElements<T>());
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
        /// Assert card has an element of the given type 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="contextTask"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<CardTestContext> AssertHas<T>(this Task<CardTestContext> contextTask, string? id = null)
            where T : AdaptiveTypedElement
        {
            var context = await contextTask;
            if (id != null)
                Assert.IsTrue(context.Card.GetElements<T>().Any(el => el.Id == id), $"{typeof(T).Name}[{id}] Not found");
            else
                Assert.IsTrue(context.Card.GetElements<T>().Any(), $"{typeof(T).Name} Not found");
            return context;
        }

        /// <summary>
        /// Assert card has an element of the given type and optionally id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="contextTask"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<CardTestContext> AssertHasNo<T>(this Task<CardTestContext> contextTask, string? id = null)
            where T : AdaptiveTypedElement
        {
            var context = await contextTask;
            if (id != null)
                Assert.IsFalse(context.Card.GetElements<T>().Any(el => el.Id == id), $"{typeof(T).Name}[Id={id}] should not be found");
            else
                Assert.IsFalse(context.Card.GetElements<T>().Any(), $"{typeof(T).Name} should not be found");
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