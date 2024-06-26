// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Crazor.Mvc.TagHelpers
{

    /// <summary>
    /// TagHelper for SelectAction
    /// </summary>

    [HtmlTargetElement("SelectAction")]
    [RestrictChildren("ActionExecute", "ActionOpenUrl", "ActionSubmit", "ActionToggleVisibility", "ActionShowCard", "ActionUnknown", "ActionOK", "ActionCancel")]
    public class SelectActionTagHelper : ReflectionTagHelper
    {
    }
}
