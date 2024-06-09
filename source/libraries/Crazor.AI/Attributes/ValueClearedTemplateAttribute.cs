// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

namespace Crazor.AI.Attributes
{

    /// <summary>
    /// Templates to describe that value was removed from a collection
    /// 0 is collection
    /// 1 is value removed
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class ValueClearedTemplateAttribute : ActionTemplateAttribute
    {
        public ValueClearedTemplateAttribute()
            : base("The ${property} value was cleared.")
        {
        }
        public ValueClearedTemplateAttribute(string template)
            : base(template)
        {
        }
    }
}
