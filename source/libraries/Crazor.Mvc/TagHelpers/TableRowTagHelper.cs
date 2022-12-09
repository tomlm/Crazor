// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel;

namespace Crazor.Mvc.TagHelpers
{

    /// <summary>
    /// TagHelper for TableRow
    /// </summary>

    [HtmlTargetElement("TableRow", ParentTag = "Table")]
    [RestrictChildren("TableCell")]
    public class TableRowTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(HorizontalAlignment))]
        [DefaultValue(typeof(AdaptiveHorizontalAlignment), "Left")]
        public AdaptiveHorizontalAlignment HorizontalAlignment { get; set; }

        [HtmlAttributeName(nameof(HorizontalCellContentAlignment))]
        [DefaultValue(typeof(AdaptiveHorizontalAlignment), "Left")]
        public AdaptiveHorizontalAlignment HorizontalCellContentAlignment { get; set; }

        [HtmlAttributeName(nameof(IsVisible))]
        public Boolean? IsVisible { get; set; } 

        [HtmlAttributeName(nameof(Separator))]
        public Boolean? Separator { get; set; }

        [HtmlAttributeName(nameof(Spacing))]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get; set; }

        [HtmlAttributeName(nameof(Style))]
        [DefaultValue(typeof(AdaptiveContainerStyle), "Default")]
        public AdaptiveContainerStyle Style { get; set; }

        [HtmlAttributeName(nameof(VerticalCellContentAlignment))]
        [DefaultValue(typeof(AdaptiveVerticalAlignment), "Top")]
        public AdaptiveVerticalAlignment VerticalCellContentAlignment { get; set; }
    }
}
