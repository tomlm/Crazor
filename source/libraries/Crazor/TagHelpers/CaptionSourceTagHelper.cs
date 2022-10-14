
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;
using Crazor.Attributes;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for CaptionSource
    /// </summary>

    [HtmlTargetElement("CaptionSource")]
    public class CaptionSourceTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(Label))]
        [DefaultValue(null)]
        public String Label { get; set; } 

        [HtmlAttributeName(nameof(MimeType))]
        [DefaultValue(null)]
        public String MimeType { get; set; } 

        [HtmlAttributeName(nameof(Url))]
        [DefaultValue(null)]
        public String Url { get; set; } 
    }
}
