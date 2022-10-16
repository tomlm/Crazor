
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;
using Crazor.Attributes;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for FactSet
    /// </summary>

    [HtmlTargetElement("FactSet")]
    [RestrictChildren("Fact")]
    public class FactSetTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(IsVisible))]
        public Boolean? IsVisible { get; set; }  

        [HtmlAttributeName(nameof(Separator))]
        public Boolean? Separator { get; set; } 

        [HtmlAttributeName(nameof(Spacing))]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get; set; } 

        [HtmlAttributeName(nameof(Speak))]
        public String Speak { get; set; } 

        [HtmlAttributeName(nameof(Height))]
        public String Height { get; set; } 
    }
}
