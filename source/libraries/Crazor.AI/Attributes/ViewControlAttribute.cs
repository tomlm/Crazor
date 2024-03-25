// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

namespace Crazor.AI.Attributes
{
    public class ViewControlAttribute : Attribute, IResponseTemplate
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ViewControlAttribute(string controlName)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            this.Control = controlName; ;
        }

        public string Control { get; set; }

        public virtual string BindTemplate(IDictionary<string, object> data)
        {
            return String.Empty;
        }
    }

}
