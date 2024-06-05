using AdaptiveCards;

namespace Crazor.Test.MSTest
{
    public static class CardContextAssertions
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
            context.Card.AssertTextBlock(id, text);
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
            context.Card.AssertTextBlock(text);
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
            context.Card.AssertNoTextBlock(text);
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
            context.Card.AssertElements<T>(callback);
            return context;
        }

        /// <summary>
        /// Assert card has an element of the given type and optionally id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="contextTask"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<CardTestContext> AssertElement<T>(this Task<CardTestContext> contextTask, string id, Action<T>? callback = null)
            where T : AdaptiveTypedElement
        {
            var context = await contextTask;
            context.Card.AssertElement<T>(id, callback);
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
            context.Card.AssertHas<T>(id);
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
            context.Card.AssertHasNo<T>(id);
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
            context.Card.AssertHasRefresh();
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
            context.Card.AssertHasNoRefresh();
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
            context.Card.AssertCard(callback);
            return context;
        }
    }
}