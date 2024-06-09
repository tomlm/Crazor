using Crazor.Test;
using Crazor.Test.MSTest;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;

namespace Crazor.Blazor.Tests
{
#pragma warning disable CS8602 
#pragma warning disable CS8604 

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
            var bot = Services.GetRequiredService<IBot>();
            var configuration = Services.GetRequiredService<IConfiguration>();
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
            var bot = Services.GetRequiredService<IBot>();
            var configuration = Services.GetRequiredService<IConfiguration>();
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
            var bot = Services.GetRequiredService<IBot>();
            var configuration = Services.GetRequiredService<IConfiguration>();
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
            card.AssertTextBlock("Counter: 1")
                .AssertTextBlock("(PREVIEW)")
                .AssertHasRefresh()
                .AssertHasNoSession()
                .AssertHasOnlyExecuteActions();
        }

        [TestMethod]
        public async Task TestTaskModulePost()
        {
            var bot = Services.GetRequiredService<IBot>();
            var configuration = Services.GetRequiredService<IConfiguration>();
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

            card.AssertTextBlock("Counter: 1")
                .AssertTextBlock("(PREVIEW)")
                .AssertHasNoRefresh()
                .AssertHasNoSession()
                .AssertHasOnlySubmitActions();

            // Click Send button
            var submitAction = card.GetElements<AdaptiveSubmitAction>().FirstOrDefault()!;
            var data = JObject.FromObject(submitAction.Data);

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
            card.AssertTextBlock("Counter: 0")
                .AssertTextBlock("(PREVIEW)")
                .AssertHasRefresh()
                .AssertHasNoSession()
                .AssertHasOnlyExecuteActions();
        }

        [TestMethod]
        public async Task TestTaskModulePostEdit()
        {
            var bot = Services.GetRequiredService<IBot>();
            var configuration = Services.GetRequiredService<IConfiguration>();
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

            card.AssertTextBlock("Counter: 1")
                .AssertTextBlock("(PREVIEW)")
                .AssertHasNoRefresh()
                .AssertHasNoSession()
                .AssertHasOnlySubmitActions();

            // Click Edit button
            var submitAction = card.GetElements<AdaptiveSubmitAction>().FirstOrDefault();
            var data = JObject.FromObject(submitAction!.Data);

            response = await adapter.Invoke(CreateMessagingExtensionSubmitActionActivity(new MessagingExtensionAction()
            {
                CommandId = "/Cards/TaskModule/Edit",
                CommandContext = "compose",
                BotActivityPreview = new List<Activity>() { preview },
                Context = new TaskModuleRequestContext() { Theme = "default" },
                BotMessagePreviewAction = "edit",
                Data = data
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
        }


        [TestMethod]
        public async Task TestTaskModuleCancel()
        {
            var bot = Services.GetRequiredService<IBot>();
            var configuration = Services.GetRequiredService<IConfiguration>();
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
        public async Task TestChoiceSetDataQuery()
        {
            var bot = Services.GetRequiredService<IBot>();
            var configuration = Services.GetRequiredService<IConfiguration>();
            var hostUri = configuration.GetValue<string>("HostUri");
            var adapter = new CardTestAdapter(bot);

            // Get card
            var response = await adapter
                .Invoke(CreateQueryLinkActivity(new AppBasedLinkQuery($"{hostUri}/Cards/DataQuery")));

            var card = response.GetCardFromResponse<MessagingExtensionResponse>();


            AdaptiveDataQuery? dq = null;
            card.AssertElement<AdaptiveChoiceSetInput>("Number", (cs) =>
            {
                Assert.AreEqual(AdaptiveChoiceInputStyle.Filtered, cs.Style);
                dq = cs.DataQuery;
            });
            Assert.IsNotNull(dq);

            // Search
            response = await adapter
                .Invoke(CreateSearchInvokeActivity(new SearchInvoke()
                {
                    Dataset = dq.Dataset,
                    Kind = "search",
                    QueryOptions = new SearchInvokeOptions()
                    {
                        Skip = dq.Skip,
                        Top = (dq.Count > 0) ? dq.Count : 10
                    },
                    QueryText = "1"
                }));

            var choices = ObjectPath.GetPathValue<List<AdaptiveChoice>>(response, "body.value.results");

            Assert.AreEqual(10, choices.Count);
            int i = 1;
            foreach (var choice in choices)
            {
                Assert.AreEqual(i.ToString(), choice.Title);
                Assert.AreEqual(i.ToString(), choice.Value);
                while (!(++i).ToString().StartsWith("1")) ;
            }

            // Search again
            response = await adapter
                .Invoke(CreateSearchInvokeActivity(new SearchInvoke()
                {
                    Dataset = dq.Dataset,
                    Kind = "search",
                    QueryOptions = new SearchInvokeOptions()
                    {
                        Skip = choices.Count,
                        Top = (dq.Count > 0) ? dq.Count : 10
                    },
                    QueryText = "1"
                }));

            choices = ObjectPath.GetPathValue<List<AdaptiveChoice>>(response, "body.value.results");
            Assert.AreEqual(10, choices.Count);

            foreach (var choice in choices)
            {
                Assert.AreEqual(i.ToString(), choice.Title);
                Assert.AreEqual(i.ToString(), choice.Value);
                while (!(++i).ToString().StartsWith("1")) ;
            }

            // Search with null results
            response = await adapter
                .Invoke(CreateSearchInvokeActivity(new SearchInvoke()
                {
                    Dataset = dq.Dataset,
                    Kind = "search",
                    QueryOptions = new SearchInvokeOptions()
                    {
                        Skip = 0xfffff,
                        Top = (dq.Count > 0) ? dq.Count : 10
                    },
                    QueryText = "1"
                }));

            choices = ObjectPath.GetPathValue<List<AdaptiveChoice>>(response, "body.value.results");
            Assert.AreEqual(0, choices.Count);
        }

        [TestMethod]
        public async Task TestSearch()
        {
            var bot = Services.GetRequiredService<IBot>();
            var configuration = Services.GetRequiredService<IConfiguration>();
            var hostUri = configuration.GetValue<string>("HostUri");
            var adapter = new CardTestAdapter(bot);

            // Search
            var response = await adapter.Invoke(CreateMessagingExtensionQueryActivity(commandId: "/Cards/Search", "st"));
            var mexResponse = ObjectPath.MapValueTo<MessagingExtensionResponse>(response.Body);

            Assert.AreEqual(2, mexResponse.ComposeExtension.Attachments.Count);
            var cards = mexResponse.ComposeExtension.Attachments.Where(a => a.ContentType == ThumbnailCard.ContentType).Select(a => ObjectPath.MapValueTo<ThumbnailCard>(a.Content)).ToList();
            Assert.AreEqual(2, cards.Count);

            Assert.AreEqual("steve", cards[0].Title);
            Assert.AreEqual("stacia", cards[1].Title);
            foreach (var card in cards)
            {
                Assert.IsNotNull(card.Tap);
                Assert.AreEqual("invoke", card.Tap.Type);
                Assert.IsTrue(ObjectPath.GetPathValue<string>(card.Tap.Value, "route").StartsWith("/Cards/Search"));
            }

            // Search past end
            response = await adapter.Invoke(CreateMessagingExtensionQueryActivity(commandId: "/Cards/Search", "st", 1000, 10000000));
            mexResponse = ObjectPath.MapValueTo<MessagingExtensionResponse>(response.Body);

            Assert.AreEqual(0, mexResponse.ComposeExtension.Attachments.Count);
            cards = mexResponse.ComposeExtension.Attachments.Where(a => a.ContentType == ThumbnailCard.ContentType).Select(a => ObjectPath.MapValueTo<ThumbnailCard>(a.Content)).ToList();
            Assert.AreEqual(0, cards.Count);
        }
    }
}