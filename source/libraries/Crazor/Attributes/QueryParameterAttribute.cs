// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Teams;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Crazor.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class QueryParameterAttribute : Attribute
    {

        public QueryParameterAttribute(string name, string description, string title)
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

        /// <summary>
        /// Type of the parameter
        /// </summary>
        public ParametersInputType InputType { get; set; } = ParametersInputType.Text;

        /// <summary>
        /// Initial value for the parameter
        /// </summary>
        public string Value { get; set; }

    }
}
