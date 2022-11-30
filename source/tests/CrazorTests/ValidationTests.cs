using AdaptiveCards;
using Crazor.Test;
using Crazor.Test.MSTest;
using CrazorTests.Validation;
using Newtonsoft.Json.Linq;

namespace CrazorTests
{
    [TestClass]
    public class ValidationTests : CardTest
    {
        [TestMethod]
        public async Task TestValidation()
        {
            await LoadCard("/Cards/Validation")
                    .AssertTextBlock(nameof(InputsModel.PhoneNumber), String.Empty)
                    .AssertTextBlock(nameof(InputsModel.Password), String.Empty)
                    .AssertTextBlock(nameof(InputsModel.Percent), String.Empty)
                    .AssertTextBlock(nameof(InputsModel.Attendees), String.Empty)
                    .AssertTextBlock(nameof(InputsModel.Birthday), String.Empty)
                    .AssertTextBlock(nameof(InputsModel.FavoritePet), String.Empty)
                    .AssertTextBlock(nameof(InputsModel.IsCool), String.Empty)
                .ExecuteAction("OnEdit")
                    .AssertHas<AdaptiveTextInput>("Model.PhoneNumber")
                    .AssertHas<AdaptiveTextInput>("Model.Password")
                    .AssertHas<AdaptiveNumberInput>("Model.Percent")
                    .AssertHas<AdaptiveNumberInput>("Model.Attendees")
                    .AssertHas<AdaptiveDateInput>("Model.Birthday")
                    .AssertHas<AdaptiveTimeInput>("Model.ArrivalTime")
                    .AssertHas<AdaptiveChoiceSetInput>("Model.FavoritePet")
                    .AssertHas<AdaptiveToggleInput>("Model.IsCool")
                .ExecuteAction("OnOK")
                    .AssertCard((card) => { })
                    .AssertTextBlock("The Birthday field is required.")
                    .AssertTextBlock("The ArrivalTime field is required.")
                    .AssertTextBlock("The Percent field is required.")
                    .AssertTextBlock("The Attendees field is required.")
                    .AssertTextBlock("Phone number is required")
                    .AssertTextBlock("Password is required")
                    .AssertTextBlock("Favorite Pet is required")
                .ExecuteAction("OnOK", new JObject()
                {
                    { "Model.Birthday", "1967-05-25"},
                })
                    .AssertNoTextBlock("The Birthday field is required.")
                    .AssertTextBlock("The ArrivalTime field is required.")
                    .AssertTextBlock("The Percent field is required.")
                    .AssertTextBlock("The Attendees field is required.")
                    .AssertTextBlock("Phone number is required")
                    .AssertTextBlock("Password is required")
                    .AssertTextBlock("Favorite Pet is required")
                .ExecuteAction("OnOK", new JObject()
                {
                    { "Model.Birthday", "1967-05-25"},
                    { "Model.ArrivalTime", "12:35"},
                    { "Model.Percent", "1.5"},
                    { "Model.Attendees", "5"},
                    { "Model.PhoneNumber", "123456789"},
                    { "Model.Password", "Test"},
                    { "Model.IsCool", "True" },
                    { "Model.FavoritePet", "Dogs" }
                })
                    .AssertNoTextBlock("The Birthday field is required.")
                    .AssertNoTextBlock("The ArrivalTime field is required.")
                    .AssertNoTextBlock("The Percent field is required.")
                    .AssertNoTextBlock("The Attendees field is required.")
                    .AssertNoTextBlock("Phone number is required")
                    .AssertNoTextBlock("Password is required")
                    .AssertNoTextBlock("Favorite Pet is required")
                    .AssertTextBlock(nameof(InputsModel.PhoneNumber), "123456789")
                    .AssertTextBlock(nameof(InputsModel.Password), "Test")
                    .AssertTextBlock(nameof(InputsModel.Percent), "1.5")
                    .AssertTextBlock(nameof(InputsModel.Attendees), "5")
                    .AssertTextBlock(nameof(InputsModel.Birthday), "5/25/1967")
                    .AssertTextBlock(nameof(InputsModel.ArrivalTime), "12:35 PM")
                    .AssertTextBlock(nameof(InputsModel.FavoritePet), "Dogs")
                    .AssertTextBlock(nameof(InputsModel.IsCool), "True");
        }
    }
}
