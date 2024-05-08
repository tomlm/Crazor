
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

        /// <summary>
        /// The name of the parameter (This is the name of the value that will be passed to your function for consumption)
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The description of the parameter
        /// </summary>
        public string Description { get; set; } = "medium";

        /// <summary>
        /// The title of the paramter
        /// </summary>
        public string Title { get; set; } = "medium";

        /// <summary>
        /// Type of the parameter
        /// </summary>
        public ParametersInputType InputType { get; set; } = ParametersInputType.Text;

        /// <summary>
        /// Initial value for the parameter
        /// </summary>
        public string? Value { get; set; }

    }
}
