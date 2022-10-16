
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;
using Crazor.Attributes;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for Choice
    /// </summary>

    [HtmlTargetElement("Choice", ParentTag ="Input.ChoiceSet")]
    public class ChoiceTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(IsSelected))]
        public Boolean? IsSelected { get; set; } 

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
