// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Attributes;

namespace Crazor.AI.Attributes
{

    /// <summary>
    /// Templates to describe that value was added to a collection
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class ValueAddedTemplateAttribute : ActionTemplateAttribute
    {
        public ValueAddedTemplateAttribute(string? template = "Added {value} to {property}")
            : base(template)
        {
        }
    }
}
