using AdaptiveCards;

namespace CrazorTests
{
    [TestClass]
    public class ActionTests : CardTest
    {
        [TestMethod]
        public async Task TestAction()
        {
            await LoadCard("/Cards/ActionTests")
                    .AssertHasRefresh()
                    .AssertTextBlock("counter", "0")
                    .AssertHas<AdaptiveExecuteAction>("OnSubmit")
                .ExecuteAction("OnSubmit")
                    .AssertHasRefresh()
                    .AssertTextBlock("counter", "1")
                    .AssertHas<AdaptiveExecuteAction>("OnSubmit")
                .ExecuteAction("OnSubmitAmount")
                    .AssertHasRefresh()
                    .AssertTextBlock("counter", "6")
                    .AssertHas<AdaptiveExecuteAction>("OnSubmit")
                .ExecuteAction("OnSubmitAmount", new { amount = 10 })
                    .AssertHasRefresh()
                    .AssertTextBlock("counter", "16")
                    .AssertHas<AdaptiveExecuteAction>("OnSubmit");
        }

        [TestMethod]
        public async Task TestPreview()
        {
            await LoadCard("/Cards/ActionTests", isPreview: true)
                    .AssertHasRefresh()
                    .AssertHas<AdaptiveTextBlock>("Preview")
                    .AssertTextBlock("PREVIEW");

            await LoadCard("/Cards/ActionTests", isPreview: false)
                    .AssertHasRefresh()
                    .AssertMissing<AdaptiveTextBlock>("Preview");
        }

    }
}
