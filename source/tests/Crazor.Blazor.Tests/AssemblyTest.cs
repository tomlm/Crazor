// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor.Blazor.Tests.Cards.CodeOnlyView;
using Crazor.Server;
using Crazor.Test;

namespace Crazor.Blazor.Tests
{
    [TestClass]
    public class AssemblyTest : CardTest
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext testContext)
        {
            InitCardServices((services) =>
            {
                services.AddCrazor();
                services.AddCrazorServer();
                services.AddCrazorBlazor();

                // add your own dependencies here...
                // services.Add<IFoo>();
            });
        }
    }
}
