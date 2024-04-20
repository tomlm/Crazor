// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor.Test;
using Crazor.Test.MSTest;

namespace Crazor.Mvc.Tests
{
    [TestClass]
    public class RouteBindingTests : CardTest
    {
        [TestMethod]
        public async Task TestRouteBinding()
        {
            await LoadCard("/Cards/RouteBinding")
                    .AssertTextBlock("ResourceId", String.Empty)
                    .AssertTextBlock("ResourceId2", String.Empty)
                    .AssertTextBlock("AppId", String.Empty)
                    .AssertTextBlock("Value", String.Empty);

            await LoadCard("/Cards/RouteBinding/app/resource")
                    .AssertTextBlock("ResourceId", "resource")
                    .AssertTextBlock("ResourceId2", "resource")
                    .AssertTextBlock("AppId", "app")
                    .AssertTextBlock("Value", String.Empty);
        }

        [TestMethod]
        public async Task TestQueryBinding()
        {
            await LoadCard("/Cards/RouteBinding?value=val")
                    .AssertTextBlock("ResourceId", String.Empty)
                    .AssertTextBlock("ResourceId2", String.Empty)
                    .AssertTextBlock("AppId", String.Empty)
                    .AssertTextBlock("Value", "val");

            await LoadCard("/Cards/RouteBinding/app/resource?value=val")
                    .AssertTextBlock("ResourceId", "resource")
                    .AssertTextBlock("ResourceId2", "resource")
                    .AssertTextBlock("AppId", "app")
                    .AssertTextBlock("Value", "val");
        }
    }
}
