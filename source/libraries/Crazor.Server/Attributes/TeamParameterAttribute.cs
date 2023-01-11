// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

namespace Crazor.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class TeamParameterAttribute : Attribute
    {

        public TeamParameterAttribute(string name, string description, string title)
        {
            this.Name = name;
            this.Description = description;
            this.Title = title;
        }

        public string? Name { get; set; }

        /// <summary>
        /// Value is [small, medium, large] or an integer which is px
        /// </summary>
        public string Description { get; set; } = "medium";

        /// <summary>
        /// Value is [small, medium, large] or an integer which is px
        /// </summary>
        public string Title { get; set; } = "medium";
    }
}
