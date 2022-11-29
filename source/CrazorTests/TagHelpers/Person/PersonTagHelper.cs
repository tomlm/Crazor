﻿using AdaptiveCards;
using Crazor.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CrazorDemoBot.TagHelpers.Person
{
    [HtmlTargetElement("Custom")]
    public class CustomTagHelper : RazorTagHelper
    {
        [HtmlAttributeName]
        public string Value { get; set; } = string.Empty;
    }
}
