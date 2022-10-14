
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for Media
    /// </summary>

    [HtmlTargetElement("Media")]
    public class MediaTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(AltText))]
        [DefaultValue(null)]
        public String AltText { get; set; } 

        [HtmlAttributeName(nameof(IsVisible))]
        [DefaultValue(true)]
        public Boolean IsVisible { get; set; }  = true;

        [HtmlAttributeName(nameof(Poster))]
        [DefaultValue(null)]
        public String Poster { get; set; } 

        [HtmlAttributeName(nameof(Separator))]
        [DefaultValue(false)]
        public Boolean Separator { get; set; } 

        [HtmlAttributeName(nameof(Spacing))]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get; set; } 

        [HtmlAttributeName(nameof(Speak))]
        [DefaultValue(null)]
        public String Speak { get; set; } 
    }
}
