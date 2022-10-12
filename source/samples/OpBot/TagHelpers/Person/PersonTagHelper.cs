using AdaptiveCards;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Crazor;

namespace OpBot.TagHelpers.Person
{
    [HtmlTargetElement("Person")]
    public class PersonTagHelper : RazorTagHelper
    {
        [HtmlAttributeName]
        public string Url { get; set; } = string.Empty;

        [HtmlAttributeName]
        public string Size { get; set; } = AdaptiveImageSize.Small.ToString();

        [HtmlAttributeName]
        public string? Name { get; set; } = string.Empty;

        [HtmlAttributeName]
        public string? Text { get; set; } = string.Empty;
    }
}
