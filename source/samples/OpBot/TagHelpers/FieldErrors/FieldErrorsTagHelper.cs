using Microsoft.AspNetCore.Razor.TagHelpers;
using Crazor;

namespace OpBot.TagHelpers.FieldErrors
{
    /// <summary>
    /// Shows errors when present for a given input id as TextBlock Attention
    /// </summary>
    [HtmlTargetElement("FieldErrors")]
    public class FieldErrorsTagHelper : RazorTagHelper
    {

        [HtmlAttributeName]
        public Dictionary<string, HashSet<string>>? Errors { get; set; }

    }
}
