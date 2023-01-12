using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Crazor.Teams
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CommandScope
    {

        [EnumMember(Value = @"team")]
        Team = 0,


        [EnumMember(Value = @"personal")]
        Personal = 1,


        [EnumMember(Value = @"groupchat")]
        Groupchat = 2,


    }
}