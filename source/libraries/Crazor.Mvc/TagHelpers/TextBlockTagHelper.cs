// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel;

namespace Crazor.Mvc.TagHelpers
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
        public Boolean? IsSubtle { get; set; } 

        [HtmlAttributeName(nameof(IsVisible))]
        public Boolean? IsVisible { get; set; }  

        [HtmlAttributeName(nameof(MaxLines))]
        public Int32? MaxLines { get; set; } 

        [HtmlAttributeName(nameof(MaxWidth))]
        public Int32? MaxWidth { get; set; } 

        [HtmlAttributeName(nameof(Separator))]
        [DefaultValue(false)]
        public Boolean? Separator { get; set; } 

        [HtmlAttributeName(nameof(Size))]
        [DefaultValue(typeof(AdaptiveTextSize), "Default")]
        public AdaptiveTextSize Size { get; set; } 

        [HtmlAttributeName(nameof(Spacing))]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get; set; } 

        [HtmlAttributeName(nameof(Speak))]
        public String Speak { get; set; } 

        [HtmlAttributeName(nameof(Style))]
        [DefaultValue(typeof(AdaptiveTextBlockStyle), "Default")]
        public AdaptiveTextBlockStyle Style { get; set; } 

        [HtmlAttributeName(nameof(Text))]
        public String Text { get; set; } 

        [HtmlAttributeName(nameof(Weight))]
        [DefaultValue(typeof(AdaptiveTextWeight), "Normal")]
        public AdaptiveTextWeight Weight { get; set; } 

        [HtmlAttributeName(nameof(Wrap))]
        public Boolean? Wrap { get; set; } 

        [HtmlAttributeName(nameof(Height))]
        public String Height { get; set; } 
    }
}
