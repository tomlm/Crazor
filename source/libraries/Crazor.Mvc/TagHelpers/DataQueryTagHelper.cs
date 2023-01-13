// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Crazor.Mvc.TagHelpers
{

    /// <summary>
    /// TagHelper for Data.Query
    /// </summary>

    [HtmlTargetElement("DataQuery", ParentTag = "Input.ChoiceSet")]
    public class DataQueryTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(Count))]
        public Int32? Count { get; set; }

        [HtmlAttributeName(nameof(Dataset))]
        public String Dataset { get; set; }

        [HtmlAttributeName(nameof(Skip))]
        public Int32? Skip { get; set; }

        [HtmlAttributeName(nameof(Value))]
        public String Value { get; set; }
        public async override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);

            output.TagName = "Data.Query";
        }
    }
}
