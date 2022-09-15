using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Bot.Cards;

namespace OpBot.TagHelpers.Accordion
{
    [HtmlTargetElement("Accordion")]
    public class AccordionTagHelper : RazorTagHelper
    {
        [HtmlAttributeName]
        public string Title { get; set; } = String.Empty;

        [HtmlAttributeName]
        public bool IsExpanded { get; set; } = false;
    }
}
