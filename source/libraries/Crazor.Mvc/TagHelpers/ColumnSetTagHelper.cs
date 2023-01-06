// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel;

namespace Crazor.Mvc.TagHelpers
{

    /// <summary>
    /// TagHelper for ColumnSet
    /// </summary>

    [HtmlTargetElement("ColumnSet")]
    [RestrictChildren("Column", "SelectAction")]
    public class ColumnSetTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(Bleed))]
        public Boolean? Bleed { get; set; }

        [HtmlAttributeName(nameof(HorizontalAlignment))]
        [DefaultValue(typeof(AdaptiveHorizontalAlignment), "Left")]
        public AdaptiveHorizontalAlignment HorizontalAlignment { get; set; }

        [HtmlAttributeName(nameof(IsVisible))]
        public Boolean? IsVisible { get; set; }

        [HtmlAttributeName(nameof(MinHeight))]
        public String MinHeight { get; set; }

        [HtmlAttributeName(nameof(Separator))]
        public Boolean? Separator { get; set; }

        [HtmlAttributeName(nameof(Spacing))]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get; set; }

        [HtmlAttributeName(nameof(Speak))]
        public String Speak { get; set; }

        [HtmlAttributeName(nameof(Style))]
        [DefaultValue(typeof(AdaptiveContainerStyle), "Default")]
        public AdaptiveContainerStyle Style { get; set; }

        [HtmlAttributeName(nameof(VerticalAlignment))]
        [DefaultValue(typeof(AdaptiveVerticalAlignment), "Top")]
        public AdaptiveVerticalAlignment VerticalAlignment { get; set; }

        [HtmlAttributeName(nameof(VerticalContentAlignment))]
        [DefaultValue(typeof(AdaptiveVerticalContentAlignment), "Top")]
        public AdaptiveVerticalContentAlignment VerticalContentAlignment { get; set; }

        [HtmlAttributeName(nameof(Height))]
        public String Height { get; set; }
    }
}
