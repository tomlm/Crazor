
using Crazor.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Crazor.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class CommandInfoAttribute : Attribute
    {
        public CommandInfoAttribute(CommandType type, string title, string description)
            : this(title, description)
        {
            Type = type;
        }

        public CommandInfoAttribute(string title, string description)
        {
            Title = title;
            Description = description;
        }

        /// <summary>
        /// Title of the command.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Type of the command
        /// </summary>
        public CommandType Type { get; set; } = CommandType.Action;

        /// <summary>
        /// Comma delmited list of Context where the command would apply
        /// </summary>
        /// <remarks>
        /// Comma delimited "message, compose"
        /// Default is "message,compose"
        /// </remarks>
        public string Context { get; set; } = "message, compose";

        /// <summary>
        /// Description of the command.
        /// </summary>
        public string Description { get; set; }
    }
}
