using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Bot.Cards;

namespace SampleWebApp.TagHelpers.FieldErrors
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
