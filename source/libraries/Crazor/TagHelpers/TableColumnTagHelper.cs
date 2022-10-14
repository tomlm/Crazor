
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for TableColumn
    /// </summary>

    [HtmlTargetElement("TableColumn")]
    public class TableColumnTagHelper : ReflectionTagHelper
    {
    }
}
