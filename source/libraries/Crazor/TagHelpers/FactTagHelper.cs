
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for Fact
    /// </summary>

    [HtmlTargetElement("Fact")]
    public class FactTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(Speak))]
        [DefaultValue(null)]
        public String Speak { get; set; } 

        [HtmlAttributeName(nameof(Title))]
        [DefaultValue(null)]
        public String Title { get; set; } 

        [HtmlAttributeName(nameof(Value))]
        [DefaultValue(null)]
        public String Value { get; set; } 
    }
}
