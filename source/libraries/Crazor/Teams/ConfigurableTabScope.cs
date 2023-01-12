using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Crazor.Teams
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ConfigurableTabScope
    {

        [EnumMember(Value = @"team")]
        Team = 0,


        [EnumMember(Value = @"groupchat")]
        Groupchat = 1,


    }
}