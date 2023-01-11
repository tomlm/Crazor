using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Crazor.Server.Teams
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DefaultInstallScope
    {

        [EnumMember(Value = @"personal")]
        Personal = 0,


        [EnumMember(Value = @"team")]
        Team = 1,


        [EnumMember(Value = @"groupchat")]
        Groupchat = 2,


        [EnumMember(Value = @"meetings")]
        Meetings = 3,


    }
}