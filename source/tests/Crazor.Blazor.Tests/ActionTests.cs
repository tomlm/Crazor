// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
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
                    .AssertElement<AdaptiveExecuteAction>("OnSubmit")
                .ExecuteAction("OnSubmit")
                    .AssertHasRefresh()
                    .AssertTextBlock("counter", "1")
                    .AssertElement<AdaptiveExecuteAction>("OnSubmit")
                .ExecuteAction("OnSubmitAmount")
                    .AssertHasRefresh()
                    .AssertTextBlock("counter", "6")
                    .AssertElement<AdaptiveExecuteAction>("OnSubmit")
                .ExecuteAction("OnSubmitAmount", new { amount = 10 })
                    .AssertHasRefresh()
                    .AssertTextBlock("counter", "16")
                    .AssertElement<AdaptiveExecuteAction>("OnSubmit");
        }

        [TestMethod]
        public async Task TestPreview()
        {
            await LoadCard("/Cards/ActionTests", isPreview: true)
                    .AssertHasRefresh()
                    .AssertElement<AdaptiveTextBlock>("Preview")
                    .AssertTextBlock("PREVIEW");

            await LoadCard("/Cards/ActionTests", isPreview: false)
                    .AssertHasRefresh()
                    .AssertHasNo<AdaptiveTextBlock>("Preview");
        }

        [TestMethod]
        public async Task TestShowCard()
        {
            await LoadCard("/Cards/ShowCard", isPreview: true)
                    .AssertElement<AdaptiveShowCardAction>("showCard")
                    .AssertTextBlock("Hello")
                    .AssertTextBlock("Hi!");
        }

        [TestMethod]
        public async Task TestSelectActionContainer()
        {
            await LoadCard("/Cards/SelectActionTests")
                    .AssertElement<AdaptiveExecuteAction>("CardAction")
                    .AssertElement<AdaptiveExecuteAction>("ContainerAction")
                    .AssertElement<AdaptiveExecuteAction>("ColumnSetAction")
                    .AssertElement<AdaptiveExecuteAction>("ColumnAction")
                    .AssertElement<AdaptiveExecuteAction>("TextRunAction")
                    .AssertElement<AdaptiveExecuteAction>("TableCellAction")
                    .AssertElement<AdaptiveExecuteAction>("ImageAction");
        }

    }
}
