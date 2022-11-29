using AdaptiveCards;
using Crazor;
using Crazor.Interfaces;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using Diag = System.Diagnostics;

namespace CrazorTests
{
    public class CardTest
    {
        static Lazy<IServiceProvider> _sp = new Lazy<IServiceProvider>(() =>
        {
            Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, "..", "..", "..");
            var builder = WebApplication.CreateBuilder();
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "HostUri", "http://localhost" },
                { "BotId", "00000000-0000-0000-0000-000000000000" }
            });
            builder.Services.AddSingleton<IRazorViewEngine, RazorViewEngine>();
            builder.Services.AddSingleton<IStorage, MemoryStorage>();
            builder.Services.AddCrazor();
            builder.Services.AddMvc()
                //                .AddRazorOptions((options) => { var x = options; })
                //                .AddRazorPagesOptions((options) => { var y = options; })
                .AddRazorRuntimeCompilation();
            var listener = new Diag.DiagnosticListener("Microsoft.AspNetCore");
            builder.Services.AddSingleton<Diag.DiagnosticListener>(listener);
            builder.Services.AddSingleton<Diag.DiagnosticSource>(listener);
            return builder.Services.BuildServiceProvider();
        });

        public static IServiceProvider Services { get => _sp.Value; }

        public static CardAppFactory Factory => Services.GetRequiredService<CardAppFactory>();

        public static Activity CreateActivity(string channelId = "test")
        {
            return new Activity()
            {
                Type = ActivityTypes.Invoke,
                ChannelId = channelId,
                Id = "test",
                From = new ChannelAccount(id: "test", name: "Test"),
                Recipient = new ChannelAccount(id: "test", name: "Test"),
                Conversation = new ConversationAccount(id: "test"),
                ServiceUrl = "http://localhost/api/messsages",
            };
        }

        /// <summary>
        /// Load card by route
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public static async Task<AdaptiveCard> LoadCard(string route, string channelId = "test", bool isPreview = false)
        {
            var cardApp = Factory.Create(CardRoute.Parse(route));

            return await cardApp.ProcessInvokeActivity(CreateActivity().CreateLoadRouteActivity(route), isPreview, default(CancellationToken));
        }
    }

    public static class CardExtensions
    {
        /// <summary>
        /// Execute Action by id
        /// </summary>
        /// <param name="cardTask"></param>
        /// <param name="idOrVerb">id of the action</param>
        /// <param name="data">optional data to merge in to simulate input controls</param>
        /// <returns></returns>
        public static async Task<AdaptiveCard> ExecuteAction(this Task<AdaptiveCard> cardTask, string idOrVerb, object data = null)
        {
            var card = await cardTask;

            var action = card.GetElements<AdaptiveExecuteAction>().SingleOrDefault(a => a.Id == idOrVerb) ??
                card.GetElements<AdaptiveExecuteAction>().First(a => a.Verb == idOrVerb);

            var combined = (JObject)JObject.FromObject(action.Data).DeepClone();
            if (data != null)
                combined.Merge(JObject.FromObject(data));

            CardRoute cardRoute = await CardRoute.FromDataAsync(combined, CardTest.Services.GetRequiredService<IEncryptionProvider>(), default(CancellationToken));
            var cardApp = CardTest.Factory.Create(cardRoute);

            return await cardApp.ProcessInvokeActivity(CardTest.CreateActivity().CreateActionInvokeActivity(action.Verb, combined), false, default(CancellationToken));
        }

        /// <summary>
        /// Assert there is a textblock[id] has text
        /// </summary>
        /// <param name="cardTask"></param>
        /// <param name="id"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static async Task<AdaptiveCard> AssertTextBlock(this Task<AdaptiveCard> cardTask, string id, string text)
        {
            var card = await cardTask;
            var actual = card.GetElements<AdaptiveTextBlock>().SingleOrDefault(el => el.Id == id)?.Text;
            Assert.AreEqual(text, actual, $"TextBlock[{id}] Expected:'{text}' Actual:'{actual}'");
            return card;
        }

        /// <summary>
        /// Assert any textblock has text 
        /// </summary>
        /// <param name="cardTask"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static async Task<AdaptiveCard> AssertTextBlock(this Task<AdaptiveCard> cardTask, string text)
        {
            var card = await cardTask;
            Assert.IsTrue(card.GetElements<AdaptiveTextBlock>().Any(el => el.Text == text), $"No TextBlock had expected:'{text}'");
            return card;
        }

        /// <summary>
        /// Assert card has an element of the given type and optionally id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cardTask"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<AdaptiveCard> AssertElement<T>(this Task<AdaptiveCard> cardTask)
            where T : AdaptiveTypedElement
        {
            var card = await cardTask;
            Assert.IsTrue(card.GetElements<AdaptiveTypedElement>().Any(), $"{typeof(T).Name} Not found");
            return card;
        }

        /// <summary>
        /// Assert card has an element of the given type and optionally id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cardTask"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<AdaptiveCard> AssertElement<T>(this Task<AdaptiveCard> cardTask, string id, Action<T> callback = null)
            where T : AdaptiveTypedElement
        {
            var card = await cardTask;
            var element = (T?)card.GetElements<AdaptiveTypedElement>().SingleOrDefault(el => el.Id == id);
            Assert.IsNotNull(element, $"{typeof(T).Name}[Id={id}] Not found");
            if (callback != null)
            {
                callback(element);
            }
            return card;
        }

        /// <summary>
        /// Assert card has an element of the given type and optionally id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cardTask"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<AdaptiveCard> AssertMissing<T>(this Task<AdaptiveCard> cardTask, string id)
            where T : AdaptiveTypedElement
        {
            var card = await cardTask;
            Assert.IsFalse(card.GetElements<AdaptiveTypedElement>().Any(el => el.Id == id), $"{typeof(T).Name}[Id={id}] should not be found");
            return card;
        }

        /// <summary>
        /// Assert refresh is valid
        /// </summary>
        /// <param name="cardTask"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static async Task<AdaptiveCard> AssertHasRefresh(this Task<AdaptiveCard> cardTask)
        {
            var card = await cardTask;
            Assert.IsNotNull(card.Refresh);
            Assert.IsNotNull(card.Refresh.Action);
            return card;
        }

        /// <summary>
        /// Assert refresh is not there
        /// </summary>
        /// <param name="cardTask"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static async Task<AdaptiveCard> AssertHasNoRefresh(this Task<AdaptiveCard> cardTask)
        {
            var card = await cardTask;
            Assert.IsNull(card.Refresh);
            return card;
        }

        /// <summary>
        /// Assert card is valid
        /// </summary>
        /// <param name="cardTask"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static async Task<AdaptiveCard> AssertCard(this Task<AdaptiveCard> cardTask, Action<AdaptiveCard> callback)
        {
            var card = await cardTask;
            callback(card);
            return card;
        }
    }
}