
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;
using Crazor.Attributes;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for Fact
    /// </summary>

    [HtmlTargetElement("Fact", ParentTag ="FactSet")]
    public class FactTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(Speak))]
        public String Speak { get; set; } 

        [HtmlAttributeName(nameof(Title))]
        public String Title { get; set; } 

        [HtmlAttributeName(nameof(Value))]
        public String Value { get; set; } 
    }
}
