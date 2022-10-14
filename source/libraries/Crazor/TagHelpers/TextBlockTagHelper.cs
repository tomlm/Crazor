
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for TextBlock
    /// </summary>

    [HtmlTargetElement("TextBlock")]
    public class TextBlockTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(Color))]
        [DefaultValue(typeof(AdaptiveTextColor), "Default")]
        public AdaptiveTextColor Color { get; set; } 

        [HtmlAttributeName(nameof(FontType))]
        [DefaultValue(typeof(AdaptiveFontType), "Default")]
        public AdaptiveFontType FontType { get; set; } 

        [HtmlAttributeName(nameof(HorizontalAlignment))]
        [DefaultValue(typeof(AdaptiveHorizontalAlignment), "Left")]
        public AdaptiveHorizontalAlignment HorizontalAlignment { get; set; } 

        [HtmlAttributeName(nameof(IsSubtle))]
        [DefaultValue(false)]
        public Boolean IsSubtle { get; set; } 

        [HtmlAttributeName(nameof(IsVisible))]
        [DefaultValue(true)]
        public Boolean IsVisible { get; set; }  = true;

        [HtmlAttributeName(nameof(Italic))]
        [DefaultValue(false)]
        public Boolean Italic { get; set; } 

        [HtmlAttributeName(nameof(MaxLines))]
        [DefaultValue(0)]
        public Int32 MaxLines { get; set; } 

        [HtmlAttributeName(nameof(MaxWidth))]
        [DefaultValue(0)]
        public Int32 MaxWidth { get; set; } 

        [HtmlAttributeName(nameof(Separator))]
        [DefaultValue(false)]
        public Boolean Separator { get; set; } 

        [HtmlAttributeName(nameof(Size))]
        [DefaultValue(typeof(AdaptiveTextSize), "Default")]
        public AdaptiveTextSize Size { get; set; } 

        [HtmlAttributeName(nameof(Spacing))]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get; set; } 

        [HtmlAttributeName(nameof(Speak))]
        [DefaultValue(null)]
        public String Speak { get; set; } 

        [HtmlAttributeName(nameof(Strikethrough))]
        [DefaultValue(false)]
        public Boolean Strikethrough { get; set; } 

        [HtmlAttributeName(nameof(Style))]
        [DefaultValue(typeof(AdaptiveTextBlockStyle), "Default")]
        public AdaptiveTextBlockStyle Style { get; set; } 

        [HtmlAttributeName(nameof(Text))]
        [DefaultValue(null)]
        public String Text { get; set; } 

        [HtmlAttributeName(nameof(TextXml))]
        [DefaultValue(null)]
        public String TextXml { get; set; } 

        [HtmlAttributeName(nameof(Weight))]
        [DefaultValue(typeof(AdaptiveTextWeight), "Normal")]
        public AdaptiveTextWeight Weight { get; set; } 

        [HtmlAttributeName(nameof(Wrap))]
        [DefaultValue(false)]
        public Boolean Wrap { get; set; } 
    }
}
