using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Crazor.Server.Teams
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MessageHandlerType
    {

        [EnumMember(Value = @"link")]
        Link = 0,
    }
}