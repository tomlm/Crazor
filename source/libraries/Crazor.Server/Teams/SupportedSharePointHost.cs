using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Crazor.Server.Teams
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SupportedSharePointHost
    {

        [EnumMember(Value = @"sharePointFullPage")]
        SharePointFullPage = 0,


        [EnumMember(Value = @"sharePointWebPart")]
        SharePointWebPart = 1,


    }
}