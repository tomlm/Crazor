// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor.Test;
using Crazor.Test.MSTest;

namespace Crazor.Mvc.Tests
{
    [TestClass]
    public class TagHelperTests : CardTest
    {
        [TestMethod]
        public async Task TestAction()
        {
            await LoadCard("/Cards/TagHelper")
                    .AssertHasRefresh()
                    .AssertTextBlock("Test1")
                    .AssertTextBlock("Test2")
                    .AssertTextBlock("InnerText");
        }
    }
}
