using Crazor.Test;
using Crazor.Test.MSTest;

namespace Crazor.Blazor.Tests
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
                    .AssertHasNo<AdaptiveTextBlock>("Preview");
        }

        [TestMethod]
        public async Task TestShowCard()
        {
            await LoadCard("/Cards/ShowCard", isPreview: true)
                    .AssertHas<AdaptiveShowCardAction>("showCard")
                    .AssertTextBlock("Hello")
                    .AssertTextBlock("Hi!");
        }

        [TestMethod]
        public async Task TestSelectActionImplicit()
        {
            await LoadCard("/Cards/SelectAction")
                    .AssertHas<AdaptiveExecuteAction>("CardAction")
                    .AssertHas<AdaptiveExecuteAction>("ContainerAction")
                    .AssertHas<AdaptiveExecuteAction>("ColumnSetAction")
                    .AssertHas<AdaptiveExecuteAction>("ColumnAction")
                    .AssertHas<AdaptiveExecuteAction>("TextRunAction")
                    .AssertHas<AdaptiveExecuteAction>("TableCellAction")
                    .AssertHas<AdaptiveExecuteAction>("ImageAction");
        }

        [TestMethod]
        public async Task TestSelectActionExplict()
        {
            await LoadCard("/Cards/SelectAction/Explicit")
                    .AssertHas<AdaptiveExecuteAction>("CardAction")
                    .AssertHas<AdaptiveExecuteAction>("ContainerAction")
                    .AssertHas<AdaptiveExecuteAction>("ColumnSetAction")
                    .AssertHas<AdaptiveExecuteAction>("ColumnAction")
                    .AssertHas<AdaptiveExecuteAction>("TextRunAction")
                    .AssertHas<AdaptiveExecuteAction>("TableCellAction")
                    .AssertHas<AdaptiveExecuteAction>("ImageAction");
        }

        [TestMethod]
        public async Task TestInlineActionImplicit()
        {
            await LoadCard("/Cards/InlineAction")
                    .AssertHas<AdaptiveExecuteAction>("InputTextAction");
        }

        [TestMethod]
        public async Task TestInlineActionExcplit()
        {
            await LoadCard("/Cards/InlineAction/Explicit")
                    .AssertHas<AdaptiveExecuteAction>("InputTextAction");
        }
    }
}
