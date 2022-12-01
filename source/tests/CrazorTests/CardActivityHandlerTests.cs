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

            var card = await adapter
                .Invoke(CreateQueryLinkActivity(new AppBasedLinkQuery($"{hostUri}/Cards/HelloWorld")))
                .GetCardFromResponse<MessagingExtensionResponse>();

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

            var card = await adapter
                .Invoke(CreateQueryLinkActivity(new AppBasedLinkQuery($"{hostUri}/Cards/HelloWorld")))
                .GetCardFromResponse<MessagingExtensionResponse>();

            card.AssertTextBlock("Hello")
                .AssertHasRefresh()
                .AssertHasNoSession();

            card = await adapter
                .Invoke(CreateAdaptiveCardActionActivity(card.GetElements<AdaptiveExecuteAction>().First()))
                .GetCardFromResponse<AdaptiveCardInvokeResponse>();

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

            var card = await adapter
                .Invoke(CreateMessagingExtensionFetchTaskAsync(new MessagingExtensionAction()
                {
                    CommandId = "/Cards/TaskModule/Edit",
                    CommandContext = "compose",
                    Context = new TaskModuleRequestContext() { Theme = "default" }
                }))
                .GetCardFromResponse<TaskModuleContinueResponse>((response) =>
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


            card = await adapter
                .Invoke(CreateMessagingExtensionSubmitActionActivity(new MessagingExtensionAction()
                {
                    CommandId = "/Cards/TaskModule/Edit",
                    CommandContext = "compose",
                    Context = new TaskModuleRequestContext() { Theme = "default" },
                    Data = card.GetElements<AdaptiveSubmitAction>().First(a => a.Id == "OnIncrement").Data
                }))
                .GetCardFromResponse<TaskModuleContinueResponse>((response) =>
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

            card = await adapter
                .Invoke(CreateMessagingExtensionSubmitActionActivity(new MessagingExtensionAction()
                {
                    CommandId = "/Cards/TaskModule/Edit",
                    CommandContext = "compose",
                    Context = new TaskModuleRequestContext() { Theme = "default" },
                    Data = card.GetElements<AdaptiveSubmitAction>().First(a => a.Id == "OnOK").Data
                }))
                .GetCardFromResponse<MessagingExtensionActionResponse>((response) =>
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
        public async Task TestTaskModuleCancel()
        {
            var bot = Services.GetService<IBot>();
            var configuration = Services.GetService<IConfiguration>();
            var hostUri = configuration.GetValue<string>("HostUri");
            var adapter = new CardTestAdapter(bot);

            var card = await adapter
                .Invoke(CreateMessagingExtensionFetchTaskAsync(new MessagingExtensionAction()
                {
                    CommandId = "/Cards/TaskModule/Edit",
                    CommandContext = "compose",
                    Context = new TaskModuleRequestContext() { Theme = "default" }
                }))
                .GetCardFromResponse<TaskModuleContinueResponse>((response) =>
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



            var response = await adapter
                .Invoke(CreateMessagingExtensionSubmitActionActivity(new MessagingExtensionAction()
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

            var invokeResponse = await adapter.Invoke(CreateTabFetchTaskActivity(new TaskModuleRequest()
            {
                TabEntityContext = new TabEntityContext() { TabEntityId = "/Cards/TabModule" }
            }));
            var tabResponse = ObjectPath.MapValueTo<TabResponse>(invokeResponse.Body);
            Assert.IsNotNull(tabResponse);
            Assert.IsNotNull(tabResponse.Tab);
            Assert.AreEqual("continue", tabResponse.Tab.Type);
            Assert.IsNotNull(tabResponse.Tab.Value);
            var card = ObjectPath.MapValueTo<AdaptiveCard>(tabResponse.Tab.Value.Cards.Single().Card);

            card.AssertTextBlock("Counter: 0")
                .AssertHasNoRefresh()
                .AssertHasSession()
                .AssertHasOnlySubmitActions();

            invokeResponse = await adapter.Invoke(CreateTabSubmitActivity(new TaskModuleRequest()
            {
                TabEntityContext = new TabEntityContext() { TabEntityId = "/Cards/TabModule" },
                Data = card.GetElements<AdaptiveSubmitAction>().Single(a => a.Id == "OnIncrement").Data,
            }));
            tabResponse = ObjectPath.MapValueTo<TabResponse>(invokeResponse.Body);
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