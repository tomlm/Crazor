// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

namespace Crazor.Attributes
{
    public enum BindingType
    {
        Value,
        PropertyName,
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
        /// if true instead of binding the the value of the property it will bind to the Name of the property.
        /// </summary>
        public BindingType Policy { get; set; } = BindingType.Value;
    }
}
