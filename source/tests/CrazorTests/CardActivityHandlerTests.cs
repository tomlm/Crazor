using AdaptiveCards;
using Crazor;
using Crazor.Test;
using Crazor.Test.MSTest;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;

namespace CrazorTests
{



    [TestClass]
    public class CardActivityHandlerTests : CardTest
    {
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
        }

        [TestMethod]
        public async Task TestQueryLink()
        {
            var bot = Services.GetService<IBot>();
            var configuration = Services.GetService<IConfiguration>();
            var hostUri = configuration.GetValue<string>("HostUri");
            var adapter = new CardTestAdapter(bot);

            var response = await adapter
                .Invoke(CreateQueryLinkActivity(new AppBasedLinkQuery($"{hostUri}/Cards/HelloWorld")));

            var card = response.GetCardFromResponse<MessagingExtensionResponse>();

            card.AssertTextBlock("Hello")
                .AssertHasRefresh()
                .AssertHasNoSession();
        }

        [TestMethod]
        public async Task TestActionInvoke()
        {
            var bot = Services.GetService<IBot>();
            var configuration = Services.GetService<IConfiguration>();
            var hostUri = configuration.GetValue<string>("HostUri");
            var adapter = new CardTestAdapter(bot);

            // Get card
            var response = await adapter
                .Invoke(CreateQueryLinkActivity(new AppBasedLinkQuery($"{hostUri}/Cards/HelloWorld")));

            var card = response.GetCardFromResponse<MessagingExtensionResponse>();

            card.AssertTextBlock("Hello")
                .AssertHasRefresh()
                .AssertHasNoSession();

            // Clicked
            response = await adapter
                .Invoke(CreateAdaptiveCardActionActivity(card.GetElements<AdaptiveExecuteAction>().First()));

            card = response.GetCardFromResponse<AdaptiveCardInvokeResponse>();

            card.AssertTextBlock("Clicked")
                .AssertHasRefresh()
                .AssertHasSession();
        }

        [TestMethod]
        public async Task TestTaskModule()
        {
            var bot = Services.GetService<IBot>();
            var configuration = Services.GetService<IConfiguration>();
            var hostUri = configuration.GetValue<string>("HostUri");
            var adapter = new CardTestAdapter(bot);

            // Get Card
            var response = await adapter.Invoke(CreateMessagingExtensionFetchTaskAsync(new MessagingExtensionAction()
            {
                CommandId = "/Cards/TaskModule/Edit",
                CommandContext = "compose",
                Context = new TaskModuleRequestContext() { Theme = "default" }
            }));

            var card = response.GetCardFromResponse<TaskModuleContinueResponse>((response) =>
            {
                Assert.AreEqual("continue", response.Type);
                Assert.AreEqual("Test Task Module", response.Value.Title);
                Assert.AreEqual("small", response.Value.Width);
                Assert.AreEqual("medium", response.Value.Height);
            });

            card.AssertTextBlock("Counter: 0")
                .AssertHasNoRefresh()
                .AssertHasSession()
                .AssertHasOnlySubmitActions();

            // OnIncrement
            response = await adapter.Invoke(CreateMessagingExtensionSubmitActionActivity(new MessagingExtensionAction()
            {
                CommandId = "/Cards/TaskModule/Edit",
                CommandContext = "compose",
                Context = new TaskModuleRequestContext() { Theme = "default" },
                Data = card.GetElements<AdaptiveSubmitAction>().First(a => a.Id == "OnIncrement").Data
            }));

            card = response.GetCardFromResponse<TaskModuleContinueResponse>((response) =>
            {
                Assert.AreEqual("continue", response.Type);
                Assert.AreEqual("Test Task Module", response.Value.Title);
                Assert.AreEqual("small", response.Value.Width);
                Assert.AreEqual("medium", response.Value.Height);
            });

            card.AssertTextBlock("Counter: 1")
                .AssertHasNoRefresh()
                .AssertHasSession()
                .AssertHasOnlySubmitActions();

            // OnOK 
            response = await adapter.Invoke(CreateMessagingExtensionSubmitActionActivity(new MessagingExtensionAction()
            {
                CommandId = "/Cards/TaskModule/Edit",
                CommandContext = "compose",
                Context = new TaskModuleRequestContext() { Theme = "default" },
                Data = card.GetElements<AdaptiveSubmitAction>().First(a => a.Id == "OnOK").Data
            }));

            card = response.GetCardFromResponse<MessagingExtensionActionResponse>((response) =>
            {
                Assert.IsNotNull(response.ComposeExtension);
            });
            card.AssertTextBlock("Counter: 1!")
                .AssertTextBlock("(PREVIEW)")
                .AssertHasRefresh()
                .AssertHasNoSession()
                .AssertHasOnlyExecuteActions();
        }

        [TestMethod]
        public async Task TestTaskModulePost()
        {
            var bot = Services.GetService<IBot>();
            var configuration = Services.GetService<IConfiguration>();
            var hostUri = configuration.GetValue<string>("HostUri");
            var adapter = new CardTestAdapter(bot);

            // fetch task module
            var response = await adapter.Invoke(CreateMessagingExtensionFetchTaskAsync(new MessagingExtensionAction()
            {
                CommandId = "/Cards/TaskModule/Edit",
                CommandContext = "compose",
                Context = new TaskModuleRequestContext() { Theme = "default" }
            }));
            var card = response.GetCardFromResponse<TaskModuleContinueResponse>((response) =>
            {
                Assert.AreEqual("continue", response.Type);
                Assert.AreEqual("Test Task Module", response.Value.Title);
                Assert.AreEqual("small", response.Value.Width);
                Assert.AreEqual("medium", response.Value.Height);
            });

            card.AssertTextBlock("Counter: 0")
                .AssertHasNoRefresh()
                .AssertHasSession()
                .AssertHasOnlySubmitActions();

            // Click OnIncrement button
            response = await adapter.Invoke(CreateMessagingExtensionSubmitActionActivity(new MessagingExtensionAction()
            {
                CommandId = "/Cards/TaskModule/Edit",
                CommandContext = "compose",
                Context = new TaskModuleRequestContext() { Theme = "default" },
                Data = card.GetElements<AdaptiveSubmitAction>().First(a => a.Id == "OnIncrement").Data
            }));

            card = response.GetCardFromResponse<TaskModuleContinueResponse>((response) =>
            {
                Assert.AreEqual("continue", response.Type);
                Assert.AreEqual("Test Task Module", response.Value.Title);
                Assert.AreEqual("small", response.Value.Width);
                Assert.AreEqual("medium", response.Value.Height);
            });

            card.AssertTextBlock("Counter: 1")
                .AssertHasNoRefresh()
                .AssertHasSession()
                .AssertHasOnlySubmitActions();

            // Click OnPost button
            response = await adapter.Invoke(CreateMessagingExtensionSubmitActionActivity(new MessagingExtensionAction()
            {
                CommandId = "/Cards/TaskModule/Edit",
                CommandContext = "compose",
                Context = new TaskModuleRequestContext() { Theme = "default" },
                Data = card.GetElements<AdaptiveSubmitAction>().First(a => a.Id == "OnPost").Data
            }));

            Activity? preview = null;
            card = response.GetCardFromResponse<MessagingExtensionActionResponse>((response) =>
            {
                Assert.AreEqual("botMessagePreview", response.ComposeExtension.Type);
                Assert.IsNotNull(response.ComposeExtension);
                Assert.IsNotNull(response.ComposeExtension.ActivityPreview);
                preview = response.ComposeExtension.ActivityPreview;
            });
            ArgumentNullException.ThrowIfNull(preview);

            card.AssertTextBlock("Counter: 1!")
                .AssertTextBlock("(PREVIEW)")
                .AssertHasRefresh()
                .AssertHasNoSession()
                .AssertHasOnlyExecuteActions();

            // Click Send button
            JObject data = new JObject();
            if (card.Refresh.Action is AdaptiveExecuteAction executeAction)
            {
                data = JObject.FromObject(executeAction.Data);
            }
            else if (card.Refresh.Action is AdaptiveSubmitAction submitAction)
            {
                data = JObject.FromObject(submitAction.Data);
            }

            response = await adapter.Invoke(CreateMessagingExtensionSubmitActionActivity(new MessagingExtensionAction()
            {
                CommandId = "/Cards/TaskModule/Edit",
                CommandContext = "compose",
                BotActivityPreview = new List<Activity>() { preview },
                Context = new TaskModuleRequestContext() { Theme = "default" },
                BotMessagePreviewAction = "send",
                Data = data
            }));
            Assert.AreEqual(200, response.Status);
            Assert.IsNull(response.Body);

            var replyActivity = adapter.ActiveQueue.Dequeue();
            var attachment = replyActivity.Attachments.Where(a => a.ContentType == AdaptiveCard.ContentType).First();
            card = ObjectPath.MapValueTo<AdaptiveCard>(attachment.Content);
            card.AssertTextBlock("Counter: 0!")
                .AssertTextBlock("(PREVIEW)")
                .AssertHasRefresh()
                .AssertHasNoSession()
                .AssertHasOnlyExecuteActions();
        }

        [TestMethod]
        public async Task TestTaskModuleCancel()
        {
            var bot = Services.GetService<IBot>();
            var configuration = Services.GetService<IConfiguration>();
            var hostUri = configuration.GetValue<string>("HostUri");
            var adapter = new CardTestAdapter(bot);

            // Get card
            var response = await adapter.Invoke(CreateMessagingExtensionFetchTaskAsync(new MessagingExtensionAction()
            {
                CommandId = "/Cards/TaskModule/Edit",
                CommandContext = "compose",
                Context = new TaskModuleRequestContext() { Theme = "default" }
            }));
            var card = response.GetCardFromResponse<TaskModuleContinueResponse>((response) =>
            {
                Assert.AreEqual("continue", response.Type);
                Assert.AreEqual("Test Task Module", response.Value.Title);
                Assert.AreEqual("small", response.Value.Width);
                Assert.AreEqual("medium", response.Value.Height);
            });

            card.AssertTextBlock("Counter: 0")
                .AssertHasNoRefresh()
                .AssertHasSession()
                .AssertHasOnlySubmitActions();

            // OnCancel 
            response = await adapter.Invoke(CreateMessagingExtensionSubmitActionActivity(new MessagingExtensionAction()
            {
                CommandId = "/Cards/TaskModule/Edit",
                CommandContext = "compose",
                Context = new TaskModuleRequestContext() { Theme = "default" },
                Data = card.GetElements<AdaptiveSubmitAction>().First(a => a.Id == "OnCancel").Data
            }));

            Assert.AreEqual(200, response.Status);
            Assert.IsNull(response.Body);
        }


        [TestMethod]
        public async Task TestTabModule()
        {
            var bot = Services.GetService<IBot>();
            var configuration = Services.GetService<IConfiguration>();
            var hostUri = configuration.GetValue<string>("HostUri");
            var adapter = new CardTestAdapter(bot);

            // Fetch Tab
            var response = await adapter.Invoke(CreateTabFetchTaskActivity(new TaskModuleRequest()
            {
                TabEntityContext = new TabEntityContext() { TabEntityId = "/Cards/TabModule" }
            }));
            var tabResponse = ObjectPath.MapValueTo<TabResponse>(response.Body);
            Assert.IsNotNull(tabResponse);
            Assert.IsNotNull(tabResponse.Tab);
            Assert.AreEqual("continue", tabResponse.Tab.Type);
            Assert.IsNotNull(tabResponse.Tab.Value);
            var card = ObjectPath.MapValueTo<AdaptiveCard>(tabResponse.Tab.Value.Cards.Single().Card);

            card.AssertTextBlock("Counter: 0")
                .AssertHasNoRefresh()
                .AssertHasSession()
                .AssertHasOnlySubmitActions();

            // OnIncrement
            response = await adapter.Invoke(CreateTabSubmitActivity(new TaskModuleRequest()
            {
                TabEntityContext = new TabEntityContext() { TabEntityId = "/Cards/TabModule" },
                Data = card.GetElements<AdaptiveSubmitAction>().Single(a => a.Id == "OnIncrement").Data,
            }));
            tabResponse = ObjectPath.MapValueTo<TabResponse>(response.Body);
            Assert.IsNotNull(tabResponse);
            Assert.IsNotNull(tabResponse.Tab);
            Assert.AreEqual("continue", tabResponse.Tab.Type);
            Assert.IsNotNull(tabResponse.Tab.Value);
            card = ObjectPath.MapValueTo<AdaptiveCard>(tabResponse.Tab.Value.Cards.Single().Card);

            card.AssertTextBlock("Counter: 1")
                .AssertHasNoRefresh()
                .AssertHasSession()
                .AssertHasOnlySubmitActions();
        }
    }
}