using Newtonsoft.Json;

namespace Crazor.Teams
{
    public class BotCommandList
    {
        /// <summary>
        /// Specifies the scopes for which the command list is valid
        /// </summary>
        [JsonProperty("scopes", Required = Required.Always)]
        public List<CommandScope> Scopes { get; set; } = new List<CommandScope>();

        [JsonProperty("commands", Required = Required.Always)]
        public List<BotCommand> Commands { get; set; } = new List<BotCommand>();


    }
}