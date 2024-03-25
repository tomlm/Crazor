// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Attributes;

namespace Crazor.AI.Attributes
{

    /// <summary>
    /// Templates to describe that value was not able to be assigned to property
    /// 0 is value attempted 
    /// 1 is error
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class ValueAssignmentFailedTemplateAttribute : ActionTemplateAttribute
    {
        public ValueAssignmentFailedTemplateAttribute()
            : base("Failed to set ${property} to ${value}: ${errors}")
        {
        }

        public ValueAssignmentFailedTemplateAttribute(string template)
            : base(template)
        {
        }
    }
}
