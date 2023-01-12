using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Crazor.Teams
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SupportedChannelType
    {

        [EnumMember(Value = @"sharedChannels")]
        SharedChannels = 0,


        [EnumMember(Value = @"privateChannels")]
        PrivateChannels = 1,


    }
}