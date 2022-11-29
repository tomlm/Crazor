using AdaptiveCards;
using Crazor.Test;

namespace CrazorTests
{
    [TestClass]
    public class ParameterBindingTests : CardTest
    {
        [TestMethod]
        public async Task TestParameterBindingLoadRoute()
        {
            await LoadCard("/Cards/ParameterBinding/LoadRoute/folder?name=joe")
                    .AssertTextBlock("Title", "folder-joe");
        }

        [TestMethod]
        public async Task TestParamterBindingOnAction()
        {
            await LoadCard("/Cards/ParameterBinding")
                    .AssertTextBlock("Xyz", String.Empty)
                .ExecuteAction("OnSubmit", new { title = "foo" })
                    .AssertTextBlock("Xyz", "foo");
        }
    }
}
