// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

namespace Crazor.AI.Attributes
{

    /// <summary>
    /// Template for when the user wasn't understood.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class NotUnderstoodTemplateAttribute : ActionTemplateAttribute
    {
        public NotUnderstoodTemplateAttribute()
            : base("I didn't uderstand that")
        {
        }

        public NotUnderstoodTemplateAttribute(string template)
            : base(template)
        {
        }
    }
}
