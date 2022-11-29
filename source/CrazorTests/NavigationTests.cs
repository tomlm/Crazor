﻿using AdaptiveCards;

namespace CrazorTests
{
    [TestClass]
    public class NavigationTests  : CardTest
    {
        [TestMethod]
        public async Task TestShowAndBack()
        {
            await LoadCard("/Cards/Navigation")
                    .AssertTextBlock("Card1")
                .ExecuteAction("OnCard2")
                    .AssertTextBlock("Card2")
                .ExecuteAction("OnBack")
                    .AssertTextBlock("Card1");
        }

        [TestMethod]
        public async Task TestReplace()
        {
            await LoadCard("/Cards/Navigation")
                    .AssertTextBlock("Card1")
                .ExecuteAction("OnCard2")
                    .AssertTextBlock("Card2")
                .ExecuteAction("OnReplace")
                    .AssertTextBlock("Card3")
                .ExecuteAction("OnBack")
                    .AssertTextBlock("Card1");
        }

        [TestMethod]
        public async Task TestShowAndBackImplicit()
        {
            await LoadCard("/Cards/Navigation")
                    .AssertTextBlock("Card1")
                .ExecuteAction("Card2")
                    .AssertTextBlock("Card2")
                .ExecuteAction("OnBack")
                    .AssertTextBlock("Card1");
        }

        [TestMethod]
        public async Task TestReplaceImplicit()
        {
            await LoadCard("/Cards/Navigation")
                    .AssertTextBlock("Card1")
                .ExecuteAction("Card2")
                    .AssertTextBlock("Card2")
                .ExecuteAction("OnReplace")
                    .AssertTextBlock("Card3")
                .ExecuteAction("OnBack")
                    .AssertTextBlock("Card1");
        }

        [TestMethod]
        public async Task TestNavigateCodeOnly()
        {
            await LoadCard("/Cards/CodeOnlyView")
                .ExecuteAction("OnShowMyCode")
                    .AssertTextBlock("CodeOnly")
                    .AssertTextBlock("Counter: 0")
                .ExecuteAction("OnIncrement")
                    .AssertTextBlock("CodeOnly")
                    .AssertTextBlock("Counter: 1")
                .ExecuteAction("OnShowView")
                    .AssertTextBlock("CodeOnly")
                    .AssertTextBlock("Counter: 1");
        }

    }
}
