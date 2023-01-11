using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Crazor.Server.Teams
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResourceSpecificType
    {

        [EnumMember(Value = @"Application")]
        Application = 0,


        [EnumMember(Value = @"Delegated")]
        Delegated = 1,


    }
}