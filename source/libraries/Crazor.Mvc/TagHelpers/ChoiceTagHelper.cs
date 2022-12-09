// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Crazor.Mvc.TagHelpers
{

    /// <summary>
    /// TagHelper for Choice
    /// </summary>

    [HtmlTargetElement("Choice", ParentTag ="Input.ChoiceSet")]
    public class ChoiceTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(IsSelected))]
        public Boolean? IsSelected { get; set; } 

        [HtmlAttributeName(nameof(Speak))]
        public String Speak { get; set; } 

        [HtmlAttributeName(nameof(Title))]
        public String Title { get; set; } 

        [HtmlAttributeName(nameof(Value))]
        public String Value { get; set; } 
    }
}
