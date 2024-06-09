// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

namespace Crazor.AI.Attributes
{

    /// <summary>
    /// Templates to describe that property was assigned a value 
    /// 0 is new value
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class ValueAssignedTemplateAttribute : ActionTemplateAttribute
    {
        public ValueAssignedTemplateAttribute()
            : base("Set ${property} to ${value}")
        {
        }

        public ValueAssignedTemplateAttribute(string template)
            : base(template)
        {
        }
    }
}
