using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Crazor.Teams
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TabContext
    {

        [EnumMember(Value = @"personalTab")]
        PersonalTab = 0,


        [EnumMember(Value = @"channelTab")]
        ChannelTab = 1,


    }
}