using Crazor.Test;
using Crazor.Test.MSTest;

namespace Crazor.Blazor.Tests
{
    [TestClass]
    public class BindPropertyTests : CardTest
    {
        [TestMethod]
        public async Task TestBindProperty()
        {
            await LoadCard("/Cards/BindProperty")
                    .AssertElement<AdaptiveTextInput>("Abc", ti => Assert.IsNull(ti.Value))
                .ExecuteAction("OnSubmit", new { Abc = "123" })
                    .AssertElement<AdaptiveTextInput>("Abc", ti => Assert.AreEqual("123", ti.Value))
                .ExecuteAction("OnSubmit")
                    .AssertElement<AdaptiveTextInput>("Abc", ti => Assert.AreEqual("123", ti.Value))
                .ExecuteAction(Constants.REFRESH_VERB)
                    .AssertElement<AdaptiveTextInput>("Abc", ti => Assert.AreEqual("123", ti.Value));
        }
    }
}
