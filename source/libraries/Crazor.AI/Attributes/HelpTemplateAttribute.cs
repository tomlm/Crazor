// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Attributes;

namespace Crazor.AI.Attributes
{

    /// <summary>
    /// Get help for a property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class HelpTemplateAttribute : ActionTemplateAttribute
    {
        public HelpTemplateAttribute(string? template = "")
            : base(template)
        {
        }
    }
}
