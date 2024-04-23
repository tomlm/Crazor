// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Crazor.Test.MSTest
{
    public static class AdaptiveCardAssertions
    {
        /// <summary>
        /// Assert there is a textblock[id] has text
        /// </summary>
        /// <param name="card"></param>
        /// <param name="id"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static AdaptiveCard AssertTextBlock(this AdaptiveCard card, string id, string text)
        {
            var actual = card.GetElements<AdaptiveTextBlock>().SingleOrDefault(el => el.Id == id)?.Text;
            Assert.AreEqual(text, actual, $"TextBlock[{id}] Expected:'{text}' Actual:'{actual}'");
            return card;
        }

        /// <summary>
        /// Assert any textblock has text 
        /// </summary>
        /// <param name="card"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static AdaptiveCard AssertTextBlock(this AdaptiveCard card, string text)
        {
            Assert.IsTrue(card.GetElements<AdaptiveTextBlock>().Any(el => el.Text == text), $"TextBlock couldn't be found with:'{text}'");
            return card;
        }

        /// <summary>
        /// Assert text block is not there
        /// </summary>
        /// <param name="card"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static AdaptiveCard AssertNoTextBlock(this AdaptiveCard card, string text)
        {
            Assert.IsFalse(card.GetElements<AdaptiveTextBlock>().Any(el => el.Text == text), $"TextBlock found with:'{text}'");
            return card;
        }

        /// <summary>
        /// Assert card has an element of the given type and optionally id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="card"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static AdaptiveCard AssertElements<T>(this AdaptiveCard card, Action<IEnumerable<T>> callback)
            where T : AdaptiveTypedElement
        {
            callback(card.GetElements<T>());
            return card;
        }

        /// <summary>
        /// Assert card has an element of the given type and optionally id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="card"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static AdaptiveCard AssertElement<T>(this AdaptiveCard card, string id, Action<T> callback = null)
            where T : AdaptiveTypedElement
        {
            var element = (T?)card.GetElements<AdaptiveTypedElement>().SingleOrDefault(el => el.Id == id);
            Assert.IsNotNull(element, $"{typeof(T).Name}[Id={id}] Not found");
            if (callback != null)
            {
                callback(element);
            }
            return card;
        }

        /// <summary>
        /// Assert card has an element of the given type 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="card"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static AdaptiveCard AssertHas<T>(this AdaptiveCard card, string? id = null)
            where T : AdaptiveTypedElement
        {
            if (id != null)
                Assert.IsTrue(card.GetElements<T>().Any(el => el.Id == id), $"{typeof(T).Name}[{id}] Not found");
            else
                Assert.IsTrue(card.GetElements<T>().Any(), $"{typeof(T).Name} Not found");
            return card;
        }

        /// <summary>
        /// Assert card has an element of the given type and optionally id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="card"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static AdaptiveCard AssertHasNo<T>(this AdaptiveCard card, string? id = null)
            where T : AdaptiveTypedElement
        {
            if (id != null)
                Assert.IsFalse(card.GetElements<T>().Any(el => el.Id == id), $"{typeof(T).Name}[Id={id}] should not be found");
            else
                Assert.IsFalse(card.GetElements<T>().Any(), $"{typeof(T).Name} should not be found");
            return card;
        }

        /// <summary>
        /// Assert refresh is valid
        /// </summary>
        /// <param name="card"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static AdaptiveCard AssertHasRefresh(this AdaptiveCard card)
        {
            Assert.IsNotNull(card.Refresh);
            Assert.IsNotNull(card.Refresh.Action);
            return card;
        }

        /// <summary>
        /// Assert refresh is not there
        /// </summary>
        /// <param name="card"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static AdaptiveCard AssertHasNoRefresh(this AdaptiveCard card)
        {
            Assert.IsNull(card.Refresh);
            return card;
        }

        /// <summary>
        /// Assert card doesn't have an action with a session key
        /// </summary>
        /// <param name="card"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static AdaptiveCard AssertHasNoSession(this AdaptiveCard card)
        {
            var action = card.GetElements<AdaptiveExecuteAction>().FirstOrDefault();
            if (action != null)
            {
                var data = JObject.FromObject(action.Data);
                Assert.IsFalse(data.ContainsKey(Constants.SESSION_KEY), "This card shouldn't have a session key defined");
            }
            return card;
        }

        /// <summary>
        /// Assert card doesn't have an action with a session key
        /// </summary>
        /// <param name="card"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static AdaptiveCard AssertHasSession(this AdaptiveCard card)
        {
            var action = card.GetElements<AdaptiveExecuteAction>().FirstOrDefault();
            if (action != null)
            {
                var data = JObject.FromObject(action.Data);
                Assert.IsTrue(data.ContainsKey(Constants.SESSION_KEY), "This card should have a session defined ");
                Assert.IsNotNull(data[Constants.SESSION_KEY], "This card should have a non null session defined");
            }
            return card;
        }

        /// <summary>
        /// Assert card has a route
        /// </summary>
        /// <param name="card"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static AdaptiveCard AssertHasRoute(this AdaptiveCard card)
        {
            var action = card.GetElements<AdaptiveExecuteAction>().FirstOrDefault();
            if (action != null)
            {
                var data = JObject.FromObject(action.Data);
                Assert.IsTrue(data.ContainsKey(Constants.ROUTE_KEY), "This card shouldn't have a session key defined");
                Assert.IsNotNull(data[Constants.ROUTE_KEY], "Route shouldn't be null");
            }
            return card;
        }

        /// <summary>
        /// Assert card is valid
        /// </summary>
        /// <param name="card"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static AdaptiveCard AssertCard(this AdaptiveCard card, Action<AdaptiveCard> callback)
        {
            callback(card);
            return card;
        }

        public static AdaptiveCard AssertHasOnlySubmitActions(this AdaptiveCard card)
        {
            Assert.IsFalse(card.GetElements<AdaptiveExecuteAction>().Any(), "Should not have an action execute in task module");
            Assert.IsTrue(card.GetElements<AdaptiveSubmitAction>().Any(), "Should have an action submit in task module");
            return card;
        }

        public static AdaptiveCard AssertHasOnlyExecuteActions(this AdaptiveCard card)
        {
            Assert.IsTrue(card.GetElements<AdaptiveExecuteAction>().Any(), "Should have an action execute in task module");
            Assert.IsFalse(card.GetElements<AdaptiveSubmitAction>().Any(), "Should not have an action submit in task module");
            return card;
        }
    }
}