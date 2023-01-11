// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Server.Teams;
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
        /// Context where the command would apply
        /// </summary>
        public List<CommandContext> Context { get; set; } = new List<CommandContext>() { CommandContext.Message, CommandContext.Compose };

        /// <summary>
        /// Description of the command.
        /// </summary>
        public string Description { get; set; }
    }
}
