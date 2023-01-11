using Newtonsoft.Json;

namespace Crazor.Server.Teams
{
    public class BotCommand
    {
        /// <summary>
        /// The bot command name
        /// </summary>
        [JsonProperty("title", Required = Required.Always)]
        public string Title { get; set; }

        /// <summary>
        /// A simple text description or an example of the command syntax and its arguments.
        /// </summary>
        [JsonProperty("description", Required = Required.Always)]
        public string Description { get; set; }


    }
}