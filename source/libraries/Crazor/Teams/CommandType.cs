using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Crazor.Teams
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CommandType
    {

        [EnumMember(Value = @"query")]
        Query = 0,


        [EnumMember(Value = @"action")]
        Action = 1,


    }
}