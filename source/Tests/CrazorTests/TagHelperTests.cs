using AdaptiveCards;
using Crazor.Test;

namespace CrazorTests
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
