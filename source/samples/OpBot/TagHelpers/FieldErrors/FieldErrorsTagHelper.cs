using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Crazor.TagHelpers;

namespace OpBot.TagHelpers.FieldErrors
{
    /// <summary>
    /// Shows errors when present for a given input id as TextBlock Attention
    /// </summary>
    [HtmlTargetElement("FieldErrors")]
    public class FieldErrorsTagHelper : RazorTagHelper
    {
    }
}
