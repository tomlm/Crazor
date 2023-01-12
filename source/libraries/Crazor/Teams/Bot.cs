using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Crazor.Teams
{
    public class Bot
    {
        /// <summary>
        /// The Microsoft App ID specified for the bot in the Bot Framework portal (https://dev.botframework.com/bots)
        /// </summary>
        [JsonProperty("botId", Required = Required.Always)]
        public string BotId { get; set; }

        /// <summary>
        /// This value describes whether or not the bot utilizes a user hint to add the bot to a specific channel.
        /// </summary>
        [JsonProperty("needsChannelSelector", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool NeedsChannelSelector { get; set; } = false;

        /// <summary>
        /// A value indicating whether or not the bot is a one-way notification only bot, as opposed to a conversational bot.
        /// </summary>
        [JsonProperty("isNotificationOnly", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool IsNotificationOnly { get; set; } = false;

        /// <summary>
        /// A value indicating whether the bot supports uploading/downloading of files.
        /// </summary>
        [JsonProperty("supportsFiles", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool SupportsFiles { get; set; } = false;

        /// <summary>
        /// A value indicating whether the bot supports audio calling.
        /// </summary>
        [JsonProperty("supportsCalling", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool SupportsCalling { get; set; } = false;

        /// <summary>
        /// A value indicating whether the bot supports video calling.
        /// </summary>
        [JsonProperty("supportsVideo", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool SupportsVideo { get; set; } = false;

        /// <summary>
        /// Specifies whether the bot offers an experience in the context of a channel in a team, in a 1:1 or group chat, or in an experience scoped to an individual user alone. These options are non-exclusive.
        /// </summary>
        [JsonProperty("scopes", Required = Required.Always)]
        public List<BotScope> Scopes { get; set; } = new List<BotScope>() { BotScope.Team, BotScope.Personal, BotScope.Groupchat };

        /// <summary>
        /// The list of commands that the bot supplies, including their usage, description, and the scope for which the commands are valid. A separate command list should be used for each scope.
        /// </summary>
        [JsonProperty("commandLists", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<BotCommandList> CommandLists { get; set; }


    }
}