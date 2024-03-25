// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Attributes;

namespace Crazor.AI.Attributes
{

    /// <summary>
    /// Templates to describe that value was removed from a collection
    /// 0 is collection
    /// 1 is value removed
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class ValueRemovedTemplateAttribute : ActionTemplateAttribute
    {
        public ValueRemovedTemplateAttribute()
            : base("Removed ${value} from ${property}.")
        {
        }

        public ValueRemovedTemplateAttribute(string template)
            : base(template)
        {
        }
    }
}
