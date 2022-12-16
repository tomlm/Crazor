// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
                // add your own dependencies here...
                // services.Add<IFoo>();
            });
        }
    }
}
