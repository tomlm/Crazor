// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

namespace Crazor.Attributes
{
    public enum BindingType
    {
        /// <summary>
        /// The value should be set to the property path's value.
        /// </summary>
        Value,

        /// <summary>
        /// The value should be set to the property path itself
        /// </summary>
        PropertyName,

        /// <summary>
        /// The value should be set to the property path's display name attribute
        /// </summary>
        DisplayName
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class BindingAttribute : Attribute
    {
        public BindingAttribute()
        {
        }

        public BindingAttribute(BindingType policy)
        {
            Policy = policy;
        }

        /// <summary>
        /// The policy for how to interpret the value of this property.
        /// </summary>
        public BindingType Policy { get; set; } = BindingType.Value;
    }
}
