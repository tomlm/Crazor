using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using Diag = System.Diagnostics;

namespace Crazor.Test
{
    public class CardTest
    {
        public static IServiceProvider Services { get; set; }

        public static CardAppFactory Factory => Services.GetRequiredService<CardAppFactory>();

        public static void InitCardServices(Action<IServiceCollection>? callback = null)
        {
            Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, "..", "..", "..");
            var builder = WebApplication.CreateBuilder();
            builder.Configuration.AddInMemoryCollection(initialData: new Dictionary<string, string?>()
            {
                { "HostUri", "http://localhost" },
                { "BotId", "00000000-0000-0000-0000-000000000000" },
                { "BotName", "TestBot" },
                { "MicrosoftAppId", "00000000-0000-0000-0000-000000000000" }
            });
            builder.Services.AddSingleton<IRazorViewEngine, RazorViewEngine>();
            builder.Services.AddSingleton<IStorage, MemoryStorage>();
            builder.Services.AddTransient<IHttpContextAccessor>((sp) => new HttpContextAccessor() { HttpContext = new DefaultHttpContext { RequestServices = sp } });
            var listener = new Diag.DiagnosticListener("Microsoft.AspNetCore");
            builder.Services.AddSingleton<Diag.DiagnosticListener>(listener);
            builder.Services.AddSingleton<Diag.DiagnosticSource>(listener);

            if (callback != null)
            {
                callback(builder.Services);
            }
            Services = builder.Services.BuildServiceProvider();
        }

        public static Activity CreateInvokeActivity(string? name = null, string channelId = "test")
        {
            return new Activity()
            {
                Type = ActivityTypes.Invoke,
                Name = name,
                ChannelId = channelId,
                Id = "test",
                From = new ChannelAccount(id: "test", name: "Test"),
                Recipient = new ChannelAccount(id: "test", name: "Test"),
                Conversation = new ConversationAccount(id: "test"),
                ServiceUrl = "http://localhost/api/messsages",
            };
        }

        public static IInvokeActivity CreateSearchInvokeActivity(SearchInvokeValue search)
        {
            var activity = CreateInvokeActivity("application/search");
            activity.Value = JObject.FromObject(search);
            return activity.AsInvokeActivity();
        }

        public static IInvokeActivity CreateAdaptiveCardActionActivity(AdaptiveCardInvokeValue actionValue)
        {
            var activity = CreateInvokeActivity("adaptiveCard/action");
            activity.Value = JObject.FromObject(actionValue);
            return activity.AsInvokeActivity();
        }

        public static IInvokeActivity CreateAdaptiveCardActionActivity(AdaptiveAction action)
        {
            return CreateAdaptiveCardActionActivity(new AdaptiveCardInvokeValue()
            {
                Action = JObject.FromObject(action).ToObject<AdaptiveCardInvokeAction>()
            });
        }

        public static IInvokeActivity CreateQueryLinkActivity(AppBasedLinkQuery linkQuery)
        {
            var activity = CreateInvokeActivity("composeExtension/queryLink");
            activity.Value = JObject.FromObject(linkQuery);
            return activity.AsInvokeActivity();
        }

        public static IInvokeActivity CreateMessagingExtensionQueryActivity(string commandId, string search, int? count = null, int? skip = null)
        {
            var activity = CreateInvokeActivity("composeExtension/query");
            activity.Value = JObject.FromObject(new MessagingExtensionQuery()
            {
                CommandId = commandId,
                QueryOptions = new MessagingExtensionQueryOptions()
                {
                    Count = count,
                    Skip = skip,
                },
                Parameters = new List<MessagingExtensionParameter>()
                {
                    new MessagingExtensionParameter() { Name = "search", Value= search }
                }
            });
            return activity.AsInvokeActivity();
        }

        public static IInvokeActivity CreateMessagingExtensionSelectItemActivity(object value)
        {
            var activity = CreateInvokeActivity("composeExtension/selectItem");
            activity.Value = JObject.FromObject(value ?? new JObject());
            return activity.AsInvokeActivity();
        }

        public static IInvokeActivity CreateMessagingExtensionSubmitActionActivity(MessagingExtensionAction action)
        {
            var activity = CreateInvokeActivity("composeExtension/submitAction");
            activity.Value = JObject.FromObject(action);
            return activity.AsInvokeActivity();
        }

        public static IInvokeActivity CreateMessagingExtensionFetchTaskAsync(MessagingExtensionAction action)
        {
            var activity = CreateInvokeActivity("composeExtension/fetchTask");
            activity.Value = JObject.FromObject(action);
            return activity.AsInvokeActivity();
        }

        public static IInvokeActivity CreateTaskModuleFetchTaskActivity(TaskModuleRequest request)
        {
            var activity = CreateInvokeActivity("task/fetch");
            activity.Value = JObject.FromObject(request);
            return activity.AsInvokeActivity();
        }

        public static IInvokeActivity CreateTaskModuleSubmitActivity(TaskModuleRequest request)
        {
            var activity = CreateInvokeActivity("task/submit");
            activity.Value = JObject.FromObject(request);
            return activity.AsInvokeActivity();
        }

        /// <summary>
        /// Load card by route
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public async Task<CardTestContext> LoadCard(string route, bool isPreview = false)
        {
            var cardRoute = CardRoute.Parse(route);
            return await LoadCard(cardRoute, isPreview: isPreview);
        }

        /// <summary>
        /// Load card by route
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public async Task<CardTestContext> LoadCard(CardRoute cardRoute, bool isPreview = false)
        {
            var activity = (Activity)CreateInvokeActivity().CreateLoadRouteActivity(cardRoute.Route);
            var cardContext = new CardTestContext()
            {
                Services = Services,
                Adapter = new TestAdapter()
            };
            await cardContext.Adapter.ProcessActivityAsync(activity, async (tc, ct) =>
                {
                    cardContext.App = Factory.Create(cardRoute, tc);
                    await cardContext.App.LoadAppAsync(activity, ct);
                    cardContext.Card = await cardContext.App.ProcessInvokeActivity(tc.Activity, isPreview, ct);
                }, default);
            return cardContext;
        }
    }
}
