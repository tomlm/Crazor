using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Crazor.Teams
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ConnectorScope
    {

        [EnumMember(Value = @"team")]
        Team = 0,


    }
}