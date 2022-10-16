
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;
using Crazor.Attributes;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for ImageSet
    /// </summary>

    [HtmlTargetElement("ImageSet")]
    [RestrictChildren("Image")]
    public class ImageSetTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(ImageSize))]
        [DefaultValue(typeof(AdaptiveImageSize), "Auto")]
        public AdaptiveImageSize ImageSize { get; set; } 

        [HtmlAttributeName(nameof(IsVisible))]
        public Boolean? IsVisible { get; set; }  

        [HtmlAttributeName(nameof(Separator))]
        public Boolean? Separator { get; set; } 

        [HtmlAttributeName(nameof(Spacing))]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get; set; } 

        [HtmlAttributeName(nameof(Speak))]
        [DefaultValue(null)]
        public String Speak { get; set; } 

        [HtmlAttributeName(nameof(Height))]
        [DefaultValue(null)]
        public String Height { get; set; } 
    }
}
