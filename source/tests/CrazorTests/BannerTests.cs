using AdaptiveCards;
using Crazor.Test;

namespace CrazorTests
{
    [TestClass]
    public class BannerTests: CardTest
    {
        [TestMethod]
        public async Task TestAddBannerMessage()
        {
            await LoadCard("/Cards/Banner")
                    .AssertHasNo<AdaptiveTextBlock>()
                .ExecuteAction("OnMessage", new { Message = "Test" })
                    .AssertTextBlock("Test")
                    .AssertElements<AdaptiveColumnSet>((columnSets) => Assert.IsTrue(columnSets.First().Style == AdaptiveContainerStyle.Accent))
                .ExecuteAction("OnMessage2", new { Message = "Test" })
                    .AssertElements<AdaptiveColumnSet>((columnSets) =>
                    {
                        Assert.IsTrue(columnSets.First().Style == AdaptiveContainerStyle.Attention);
                        Assert.IsTrue(columnSets.Skip(1).First().Style == AdaptiveContainerStyle.Accent);
                    })
                .ExecuteAction("OnShowView")
                    .AssertHasNo<AdaptiveTextBlock>();
        }
    }
}
