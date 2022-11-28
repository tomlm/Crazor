// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for MediaSource
    /// </summary>

    [HtmlTargetElement("MediaSource")]
    public class MediaSourceTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(MimeType))]
        public String MimeType { get; set; } 

        [HtmlAttributeName(nameof(Url))]
        public String Url { get; set; } 
    }
}
