
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;
using Crazor.Attributes;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for Image
    /// </summary>

    [HtmlTargetElement("Image")]
    public class ImageTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(AltText))]
        public String AltText { get; set; } 

        [HtmlAttributeName(nameof(BackgroundColor))]
        public String BackgroundColor { get; set; } 

        [HtmlAttributeName(nameof(HorizontalAlignment))]
        [DefaultValue(typeof(AdaptiveHorizontalAlignment), "Left")]
        public AdaptiveHorizontalAlignment HorizontalAlignment { get; set; } 

        [HtmlAttributeName(nameof(IsVisible))]
        public Boolean? IsVisible { get; set; }  

        [HtmlAttributeName(nameof(Separator))]
        public Boolean? Separator { get; set; } 

        [HtmlAttributeName(nameof(Size))]
        [DefaultValue(typeof(AdaptiveImageSize), "Auto")]
        public AdaptiveImageSize Size { get; set; } 

        [HtmlAttributeName(nameof(Spacing))]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get; set; } 

        [HtmlAttributeName(nameof(Speak))]
        public String Speak { get; set; } 

        [HtmlAttributeName(nameof(Style))]
        [DefaultValue(typeof(AdaptiveImageStyle), "Normal")]
        public AdaptiveImageStyle Style { get; set; } 

        [HtmlAttributeName(nameof(Url))]
        public String Url { get; set; } 

        [HtmlAttributeName(nameof(Height))]
        public String Height { get; set; } 

        [HtmlAttributeName(nameof(Width))]
        public String Width { get; set; } 
    }
}
