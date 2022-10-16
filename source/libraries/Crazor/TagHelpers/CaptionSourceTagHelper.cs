
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
        public String Label { get; set; } 

        [HtmlAttributeName(nameof(MimeType))]
        public String MimeType { get; set; } 

        [HtmlAttributeName(nameof(Url))]
        public String Url { get; set; } 
    }
}
