using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Crazor.Teams
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Context
    {

        [EnumMember(Value = @"personalTab")]
        PersonalTab = 0,


        [EnumMember(Value = @"channelTab")]
        ChannelTab = 1,


        [EnumMember(Value = @"privateChatTab")]
        PrivateChatTab = 2,


        [EnumMember(Value = @"meetingChatTab")]
        MeetingChatTab = 3,


        [EnumMember(Value = @"meetingDetailsTab")]
        MeetingDetailsTab = 4,


        [EnumMember(Value = @"meetingSidePanel")]
        MeetingSidePanel = 5,


        [EnumMember(Value = @"meetingStage")]
        MeetingStage = 6,


        [EnumMember(Value = @"callingSidePanel")]
        CallingSidePanel = 7,


    }
}