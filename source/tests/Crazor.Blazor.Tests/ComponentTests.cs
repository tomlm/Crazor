


using Crazor.Test;
using Crazor.Test.MSTest;

namespace Crazor.Blazor.Tests
{
    [TestClass]
    public class CardTests : CardTest
    {
        [TestMethod]
        public async Task TestColumnSet()
        {
            await LoadCard("/Cards/Columns")
                    .AssertTextBlock("Hello World");
        }
    }
}
