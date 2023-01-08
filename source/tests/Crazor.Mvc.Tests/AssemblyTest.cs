// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor.Mvc.Tests.Cards.CodeOnlyView;
using Crazor.Server;
using Crazor.Test;

namespace Crazor.Mvc.Tests
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
                services.AddCrazorMvc();
                services.AddCardView<MyCodeView>();
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
