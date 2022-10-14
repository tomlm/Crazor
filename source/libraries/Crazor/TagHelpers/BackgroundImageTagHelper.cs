
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for BackgroundImage
    /// </summary>

    [HtmlTargetElement("BackgroundImage")]
    public class BackgroundImageTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(FillMode))]
        [DefaultValue(typeof(AdaptiveImageFillMode), "Cover")]
        public AdaptiveImageFillMode FillMode { get; set; } 

        [HtmlAttributeName(nameof(HorizontalAlignment))]
        [DefaultValue(typeof(AdaptiveHorizontalAlignment), "Left")]
        public AdaptiveHorizontalAlignment HorizontalAlignment { get; set; } 

        [HtmlAttributeName(nameof(Url))]
        [DefaultValue(null)]
        public String Url { get; set; } 

        [HtmlAttributeName(nameof(VerticalAlignment))]
        [DefaultValue(typeof(AdaptiveVerticalAlignment), "Top")]
        public AdaptiveVerticalAlignment VerticalAlignment { get; set; } 
    }
}
