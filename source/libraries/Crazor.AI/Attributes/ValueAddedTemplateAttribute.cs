// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

namespace Crazor.AI.Attributes
{

    /// <summary>
    /// Templates to describe that value was added to a collection
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class ValueAddedTemplateAttribute : ActionTemplateAttribute
    {
        public ValueAddedTemplateAttribute()
            : base("Added ${value} to ${property}")
        { }

        public ValueAddedTemplateAttribute(string? template)
            : base(template)
        {
        }
    }
}
