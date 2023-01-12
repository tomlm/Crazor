using Newtonsoft.Json;

namespace Crazor.Teams
{
    public class ComposeExtension
    {
        /// <summary>
        /// The Microsoft App ID specified for the bot powering the compose extension in the Bot Framework portal (https://dev.botframework.com/bots)
        /// </summary>
        [JsonProperty("botId", Required = Required.Always)]
        public string BotId { get; set; }

        /// <summary>
        /// A value indicating whether the configuration of a compose extension can be updated by the user.
        /// </summary>
        [JsonProperty("canUpdateConfiguration", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool CanUpdateConfiguration { get; set; } = false;

        [JsonProperty("commands", Required = Required.Always)]
        public List<Command> Commands { get; set; } = new List<Command>();

        /// <summary>
        /// A list of handlers that allow apps to be invoked when certain conditions are met
        /// </summary>
        [JsonProperty("messageHandlers", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<MessageHandler> MessageHandlers { get; set; } = new List<MessageHandler>();
    }
}