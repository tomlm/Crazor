using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Crazor.Server.Teams
{
    public class DefaultGroupCapability
    {
        /// <summary>
        /// When the install scope selected is Team, this field specifies the default capability available
        /// </summary>
        [JsonProperty("team", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public DefaultGroupCapabilityTeam Team { get; set; }

        /// <summary>
        /// When the install scope selected is GroupChat, this field specifies the default capability available
        /// </summary>
        [JsonProperty("groupchat", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public DefaultGroupCapabilityGroupchat Groupchat { get; set; }

        /// <summary>
        /// When the install scope selected is Meetings, this field specifies the default capability available
        /// </summary>
        [JsonProperty("meetings", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public DefaultGroupCapabilityMeeting Meetings { get; set; }


    }
}