// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor.Mvc;
using Crazor.Test;

namespace Crazor.Tests
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
                services.AddMvc()
                    // .AddRazorOptions((options) => { var x = options; })
                    // .AddRazorPagesOptions((options) => { var y = options; })
                    .AddRazorRuntimeCompilation();
                // add your own dependencies here...
                // services.Add<IFoo>();
            });
        }
    }
}
