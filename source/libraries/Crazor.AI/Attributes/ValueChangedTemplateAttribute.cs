﻿// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Attributes;

namespace Crazor.AI.Attributes
{

    /// <summary>
    /// Templates to describe that property has changed from old value to new value
    /// 0 is new value
    /// 1 is old value
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class ValueChangedTemplateAttribute : ActionTemplateAttribute
    {
        public ValueChangedTemplateAttribute(string? template = "Changed {property} from {oldValue} to {value}.")
            : base(template)
        {
        }
    }
}
