
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;
using Crazor.Attributes;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for Input.Toggle
    /// </summary>

    [HtmlTargetElement("Input.Toggle")]
    public class InputToggleTagHelper : InputTagHelper
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

        [HtmlAttributeName(nameof(Title))]
        public String Title { get; set; } 

        [HtmlAttributeName(nameof(Value))]
        [Binding(BindingType.Value)]
        public String Value { get; set; } 

        [HtmlAttributeName(nameof(ValueOff))]
        public String ValueOff { get; set; } 

        [HtmlAttributeName(nameof(ValueOn))]
        public String ValueOn { get; set; } 

        [HtmlAttributeName(nameof(Wrap))]
        public Boolean? Wrap { get; set; } 

        [HtmlAttributeName(nameof(Height))]
        public String Height { get; set; } 
    }
}
