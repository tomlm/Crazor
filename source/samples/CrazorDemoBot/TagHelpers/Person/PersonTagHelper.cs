using AdaptiveCards;
using Crazor.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CrazorDemoBot.TagHelpers.Person
{
    [HtmlTargetElement("Person")]
    public class PersonTagHelper : RazorTagHelper
    {
        [HtmlAttributeName]
        public string Url { get; set; } = string.Empty;

        [HtmlAttributeName]
        public AdaptiveImageSize Size { get; set; } = AdaptiveImageSize.Small;

        [HtmlAttributeName]
        public string? Name { get; set; } = string.Empty;

        [HtmlAttributeName]
        public string? Text { get; set; } = string.Empty;
    }
}
