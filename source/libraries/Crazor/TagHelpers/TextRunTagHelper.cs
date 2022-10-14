
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;
using Crazor.Attributes;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for TextRun
    /// </summary>

    [HtmlTargetElement("TextRun")]
    public class TextRunTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(Color))]
        [DefaultValue(typeof(AdaptiveTextColor), "Default")]
        public AdaptiveTextColor Color { get; set; } 

        [HtmlAttributeName(nameof(FontType))]
        [DefaultValue(typeof(AdaptiveFontType), "Default")]
        public AdaptiveFontType FontType { get; set; } 

        [HtmlAttributeName(nameof(Highlight))]
        [DefaultValue(false)]
        public Boolean Highlight { get; set; } 

        [HtmlAttributeName(nameof(IsSubtle))]
        [DefaultValue(false)]
        public Boolean IsSubtle { get; set; } 

        [HtmlAttributeName(nameof(Italic))]
        [DefaultValue(false)]
        public Boolean Italic { get; set; } 

        [HtmlAttributeName(nameof(Size))]
        [DefaultValue(typeof(AdaptiveTextSize), "Default")]
        public AdaptiveTextSize Size { get; set; } 

        [HtmlAttributeName(nameof(Strikethrough))]
        [DefaultValue(false)]
        public Boolean Strikethrough { get; set; } 

        [HtmlAttributeName(nameof(Underline))]
        [DefaultValue(false)]
        public Boolean Underline { get; set; } 

        [HtmlAttributeName(nameof(Weight))]
        [DefaultValue(typeof(AdaptiveTextWeight), "Normal")]
        public AdaptiveTextWeight Weight { get; set; } 
    }
}
