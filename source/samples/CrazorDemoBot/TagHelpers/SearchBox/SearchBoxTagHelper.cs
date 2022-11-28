using Crazor.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CrazorDemoBot.TagHelpers.SearchBox
{
    [HtmlTargetElement("SearchBox")]
    public class SearchBoxTagHelper : RazorTagHelper
    {
        /// <summary>
        /// Title for search button
        /// </summary>
        [HtmlAttributeName(nameof(Title))]
        public string? Title { get; set; }

        /// <summary>
        /// Verb for search button (Default is OnSearch)
        /// </summary>
        [HtmlAttributeName(nameof(Verb))]
        public string? Verb { get; set; } = "OnSearch";

        /// <summary>
        /// Binding for Input.Id
        /// </summary>
        [HtmlAttributeName(nameof(Id))]
        public string? Id { get; set; }

        /// <summary>
        /// Binding for Input.Value
        /// </summary>
        [HtmlAttributeName(nameof(Value))]
        public string? Value { get; set; }


        /// <summary>
        /// Label for Text.Input
        /// </summary>
        [HtmlAttributeName(nameof(Label))]
        public string? Label { get; set; }

        /// <summary>
        /// Placeholder for Text.Input (Default is "Enter search text")
        /// </summary>
        [HtmlAttributeName(nameof(Placeholder))]
        public string? Placeholder { get; set; } = "Enter search text";
    }
}
