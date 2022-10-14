
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;
using Crazor.Attributes;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for Input.Time
    /// </summary>

    [HtmlTargetElement("Input.Time")]
    public class InputTimeTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(ErrorMessage))]
        [DefaultValue(null)]
        public String ErrorMessage { get; set; } 

        [HtmlAttributeName(nameof(IsRequired))]
        [DefaultValue(false)]
        public Boolean IsRequired { get; set; } 

        [HtmlAttributeName(nameof(IsVisible))]
        [DefaultValue(true)]
        public Boolean IsVisible { get; set; }  = true;

        [HtmlAttributeName(nameof(Label))]
        [DefaultValue(null)]
        [BindValue(BindingType.DisplayName)]
        public String Label { get; set; } 

        [HtmlAttributeName(nameof(Max))]
        [DefaultValue(null)]
        public String Max { get; set; } 

        [HtmlAttributeName(nameof(Min))]
        [DefaultValue(null)]
        public String Min { get; set; } 

        [HtmlAttributeName(nameof(Placeholder))]
        [DefaultValue(null)]
        public String Placeholder { get; set; } 

        [HtmlAttributeName(nameof(Separator))]
        [DefaultValue(false)]
        public Boolean Separator { get; set; } 

        [HtmlAttributeName(nameof(Spacing))]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get; set; } 

        [HtmlAttributeName(nameof(Speak))]
        [DefaultValue(null)]
        public String Speak { get; set; } 

        [HtmlAttributeName(nameof(Value))]
        [DefaultValue(null)]
        [BindValue]
        public String Value { get; set; } 

        [HtmlAttributeName(nameof(Height))]
        [DefaultValue(null)]
        public String Height { get; set; } 
    }
}
