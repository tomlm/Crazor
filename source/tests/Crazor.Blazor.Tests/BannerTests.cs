using Crazor.Test;
using Crazor.Test.MSTest;

namespace Crazor.Blazor.Tests
{
    [TestClass]
    public class BannerTests : CardTest
    {
        [TestMethod]
        public async Task TestAddBannerMessage()
        {
            await LoadCard("/Cards/Banner")
                    .AssertHasNo<AdaptiveColumnSet>("messageBanner0")
                    .AssertHasNo<AdaptiveColumnSet>("messageBanner1")
                .ExecuteAction("OnMessage", new { Message = "Test" })
                    .AssertHas<AdaptiveColumnSet>("messageBanner0")
                    .AssertHasNo<AdaptiveColumnSet>("messageBanner1")
                    .AssertElement<AdaptiveColumnSet>("messageBanner0", (columnSet) => Assert.IsTrue(columnSet.Style == AdaptiveContainerStyle.Accent))
                .ExecuteAction("OnMessage2", new { Message = "Test" })
                    .AssertHas<AdaptiveColumnSet>("messageBanner0")
                    .AssertHas<AdaptiveColumnSet>("messageBanner1")
                    .AssertElement<AdaptiveColumnSet>("messageBanner0", (columnSet) => Assert.IsTrue(columnSet.Style == AdaptiveContainerStyle.Accent))
                    .AssertElement<AdaptiveColumnSet>("messageBanner1", (columnSet) => Assert.IsTrue(columnSet.Style == AdaptiveContainerStyle.Attention))
                .ExecuteAction(Constants.REFRESH_VERB)
                    .AssertHasNo<AdaptiveColumnSet>("messageBanner0")
                    .AssertHasNo<AdaptiveColumnSet>("messageBanner1");
        }
    }
}
