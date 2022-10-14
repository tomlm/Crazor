
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for Image
    /// </summary>

    [HtmlTargetElement("Image")]
    public class ImageTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(AltText))]
        [DefaultValue(null)]
        public String AltText { get; set; } 

        [HtmlAttributeName(nameof(BackgroundColor))]
        [DefaultValue(null)]
        public String BackgroundColor { get; set; } 

        [HtmlAttributeName(nameof(HorizontalAlignment))]
        [DefaultValue(typeof(AdaptiveHorizontalAlignment), "Left")]
        public AdaptiveHorizontalAlignment HorizontalAlignment { get; set; } 

        [HtmlAttributeName(nameof(IsVisible))]
        [DefaultValue(true)]
        public Boolean IsVisible { get; set; }  = true;

        [HtmlAttributeName(nameof(Separator))]
        [DefaultValue(false)]
        public Boolean Separator { get; set; } 

        [HtmlAttributeName(nameof(Size))]
        [DefaultValue(typeof(AdaptiveImageSize), "Auto")]
        public AdaptiveImageSize Size { get; set; } 

        [HtmlAttributeName(nameof(Spacing))]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get; set; } 

        [HtmlAttributeName(nameof(Speak))]
        [DefaultValue(null)]
        public String Speak { get; set; } 

        [HtmlAttributeName(nameof(Style))]
        [DefaultValue(typeof(AdaptiveImageStyle), "Normal")]
        public AdaptiveImageStyle Style { get; set; } 

        [HtmlAttributeName(nameof(Url))]
        [DefaultValue(null)]
        public String Url { get; set; } 
    }
}
