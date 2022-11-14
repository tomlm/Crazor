
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for TargetElement
    /// </summary>

    [HtmlTargetElement("TargetElement")]
    public class TargetElementTagHelper : ReflectionTagHelper
    {

        /// <summary>
        /// Target element Id.
        /// </summary>
        [HtmlAttributeName(nameof(ElementId))]
        public string ElementId { get; set; }

        /// <summary>
        /// Target element visibility.
        /// </summary>
        [HtmlAttributeName(nameof(IsVisible))]
        [DefaultValue(null)]
        public bool? IsVisible { get; set; } = null;

    }
}
