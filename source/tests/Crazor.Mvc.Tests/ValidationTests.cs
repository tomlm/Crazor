// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using Crazor.Mvc.Tests.Validation;
using Crazor.Test;
using Crazor.Test.MSTest;
using Newtonsoft.Json.Linq;

namespace Crazor.Mvc.Tests
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
                .ExecuteAction(Constants.ONEDIT_VERB)
                    .AssertHas<AdaptiveTextInput>("Model.PhoneNumber")
                    .AssertHas<AdaptiveTextInput>("Model.Password")
                    .AssertHas<AdaptiveNumberInput>("Model.Percent")
                    .AssertHas<AdaptiveNumberInput>("Model.Attendees")
                    .AssertHas<AdaptiveDateInput>("Model.Birthday")
                    .AssertHas<AdaptiveTimeInput>("Model.ArrivalTime")
                    .AssertHas<AdaptiveChoiceSetInput>("Model.FavoritePet")
                    .AssertHas<AdaptiveToggleInput>("Model.IsCool")
                    .AssertElement<AdaptiveTextInput>("Model.PhoneNumber", (el) =>
                    {
                        Assert.AreEqual(true, el.IsRequired);
                        Assert.AreEqual(AdaptiveTextInputStyle.Tel, el.Style);
                        Assert.AreEqual("Phone number is required", el.ErrorMessage);
                    })
                    .AssertElement<AdaptiveTextInput>("Model.Password", (el) =>
                    {
                        Assert.AreEqual(true, el.IsRequired);
                        Assert.AreEqual(AdaptiveTextInputStyle.Password, el.Style);
                        Assert.AreEqual("Password is required", el.ErrorMessage);
                    })
                    .AssertElement<AdaptiveNumberInput>("Model.Percent", (el) =>
                    {
                        Assert.AreEqual(true, el.IsRequired);
                        Assert.AreEqual(0f, el.Min);
                        Assert.AreEqual(100f, el.Max);
                        Assert.AreEqual("Percentage must be between 0 and 100.", el.ErrorMessage);
                    })
                    .AssertElement<AdaptiveNumberInput>("Model.Attendees", (el) =>
                    {
                        Assert.AreEqual(true, el.IsRequired);
                        Assert.AreEqual(0f, el.Min);
                        Assert.AreEqual(100f, el.Max);
                        Assert.AreEqual("Attendees must be between 0 and 100.", el.ErrorMessage);
                    })
                    .AssertElement<AdaptiveDateInput>("Model.Birthday", (el) =>
                    {
                        Assert.AreEqual(true, el.IsRequired);
                        Assert.AreEqual("1900-01-01", el.Min);
                        Assert.IsTrue(DateTime.Parse(el.Max) < DateTime.Now);
                        Assert.AreEqual("Birthday has to be between 1900 and 2022", el.ErrorMessage);
                    })
                    .AssertElement<AdaptiveTimeInput>("Model.ArrivalTime", (el) =>
                    {
                        Assert.AreEqual(true, el.IsRequired);
                        // Assert.AreEqual("08:00", el.Min);
                        Assert.AreEqual("20:00", el.Max);
                        Assert.AreEqual("Arrival time must be between 8AM and 8PM", el.ErrorMessage);
                    })
                    .AssertElement<AdaptiveChoiceSetInput>("Model.FavoritePet", (el) =>
                    {
                        Assert.AreEqual(true, el.IsRequired);
                        Assert.AreEqual("Favorite Pet is required", el.ErrorMessage);
                    })
                    .AssertElement<AdaptiveToggleInput>("Model.IsCool", (el) =>
                    {
                        Assert.AreEqual(true, el.IsRequired);
                        Assert.AreEqual("Cool is required", el.ErrorMessage);
                    })
                .ExecuteAction("OnOK")
                    .AssertCard((card) => { })
                    .AssertTextBlock("The Birthday field is required.")
                    .AssertTextBlock("The ArrivalTime field is required.")
                    .AssertTextBlock("The Percent field is required.")
                    .AssertTextBlock("The Attendees field is required.")
                    .AssertTextBlock("Phone number is required")
                    .AssertTextBlock("Password is required")
                    .AssertTextBlock("Cool is required")
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
                    .AssertTextBlock("Cool is required")
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
                    .AssertNoTextBlock("Cool is required")
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
