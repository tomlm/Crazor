using AdaptiveCards;
using Crazor.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CrazorDemoBot.TagHelpers.ThumbnailCard
{
    [HtmlTargetElement("ThumbnailCard")]
    public class ThumbnailCardTagHelper : RazorTagHelper
    {
        [HtmlAttributeName(nameof(ImageUrl))]
        public string? ImageUrl { get; set; }

        [HtmlAttributeName(nameof(Size))]
        public AdaptiveImageSize? Size { get; set; }

        [HtmlAttributeName(nameof(Title))]
        public string? Title { get; set; }

        [HtmlAttributeName(nameof(Subtitle))]
        public string? Subtitle { get; set; } 
    }
}
