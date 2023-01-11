using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Crazor.Server.Teams
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StaticTabScope
    {

        [EnumMember(Value = @"team")]
        Team = 0,


        [EnumMember(Value = @"personal")]
        Personal = 1,


    }
}