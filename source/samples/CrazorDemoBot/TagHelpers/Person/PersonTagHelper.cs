// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor.AdaptiveCards;
using Crazor.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CrazorDemoBot.TagHelpers.Person
{
    [HtmlTargetElement("Person")]
    public class PersonTagHelper : RazorTagHelper
    {
        [HtmlAttributeName]
        public string Url { get; set; } = string.Empty;

        [HtmlAttributeName]
        public AdaptiveImageSize Size { get; set; } = AdaptiveImageSize.Small;

        [HtmlAttributeName]
        public string? Name { get; set; } = string.Empty;

        [HtmlAttributeName]
        public string? Text { get; set; } = string.Empty;
    }
}
