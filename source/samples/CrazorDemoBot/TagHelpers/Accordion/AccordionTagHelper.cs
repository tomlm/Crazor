// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CrazorDemoBot.TagHelpers.Accordion
{
    [HtmlTargetElement("Accordion")]
    public class AccordionTagHelper : RazorTagHelper
    {
        [HtmlAttributeName(nameof(Title))]
        public string Title { get; set; } = String.Empty;

        [HtmlAttributeName(nameof(IsExpanded))]
        public bool IsExpanded { get; set; } = false;
    }
}
