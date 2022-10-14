
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;
using Crazor.Attributes;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for TableColumn
    /// </summary>

    [HtmlTargetElement("TableColumn", ParentTag ="Table")]
    public class TableColumnTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(Width))]
        [DefaultValue(null)]
        public String Width { get; set; } 
    }
}
