// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel;

namespace Crazor.Mvc.TagHelpers
{

    /// <summary>
    /// TagHelper for ActionSet
    /// </summary>

    [HtmlTargetElement("ActionSet")]
    [RestrictChildren("Action.Execute", "Action.OpenUrl", "Action.Submit", "Action.ToggleVisibility", "Action.ShowCard", "Action.Unknown")]
    public class ActionSetTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(IsVisible))]
        public Boolean? IsVisible { get; set; }

        [HtmlAttributeName(nameof(Separator))]
        public Boolean? Separator { get; set; }

        [HtmlAttributeName(nameof(Spacing))]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get; set; }

        [HtmlAttributeName(nameof(Speak))]
        public String Speak { get; set; }

        [HtmlAttributeName(nameof(Height))]
        public String Height { get; set; }
    }
}
