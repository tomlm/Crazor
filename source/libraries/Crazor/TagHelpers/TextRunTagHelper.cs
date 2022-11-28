// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for TextRun
    /// </summary>

    [HtmlTargetElement("TextRun", ParentTag = "RichTExtBlock")]
    public class TextRunTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(Color))]
        [DefaultValue(typeof(AdaptiveTextColor), "Default")]
        public AdaptiveTextColor Color { get; set; }

        [HtmlAttributeName(nameof(FontType))]
        [DefaultValue(typeof(AdaptiveFontType), "Default")]
        public AdaptiveFontType FontType { get; set; }

        [HtmlAttributeName(nameof(Highlight))]
        public Boolean? Highlight { get; set; }

        [HtmlAttributeName(nameof(IsSubtle))]
        public Boolean? IsSubtle { get; set; }

        [HtmlAttributeName(nameof(Italic))]
        public Boolean? Italic { get; set; }

        [HtmlAttributeName(nameof(Size))]
        [DefaultValue(typeof(AdaptiveTextSize), "Default")]
        public AdaptiveTextSize Size { get; set; }

        [HtmlAttributeName(nameof(Strikethrough))]
        public Boolean? Strikethrough { get; set; }

        [HtmlAttributeName(nameof(Underline))]
        public Boolean? Underline { get; set; }

        [HtmlAttributeName(nameof(Weight))]
        [DefaultValue(typeof(AdaptiveTextWeight), "Normal")]
        public AdaptiveTextWeight Weight { get; set; }
    }
}
