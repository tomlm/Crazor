// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for SelectAction
    /// </summary>

    [HtmlTargetElement("SelectAction")]
    [RestrictChildren("Action.Execute", "Action.Submit", "Action.OpenUrl", "Action.ShowCard", "Action.ToggleVisibility", "Action.Unknown")]
    public class SelectActionTagHelper : ReflectionTagHelper
    {
    }
}
