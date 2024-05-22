using Crazor.Test;
using Crazor.Test.MSTest;

namespace Crazor.Blazor.Tests
{
    [TestClass]
    public class CustomComponentTest : CardTest
    {
        [TestMethod]
        public async Task TestAction()
        {
            await LoadCard("/Cards/CustomComponent")
                    .AssertHasRefresh()
                    .AssertTextBlock("Test1")
                    .AssertTextBlock("Test2")
                    .AssertTextBlock("InnerText");
        }
    }
}
