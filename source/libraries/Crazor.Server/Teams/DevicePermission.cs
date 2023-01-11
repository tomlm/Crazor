using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Crazor.Server.Teams
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DevicePermission
    {

        [EnumMember(Value = @"geolocation")]
        Geolocation = 0,


        [EnumMember(Value = @"media")]
        Media = 1,


        [EnumMember(Value = @"notifications")]
        Notifications = 2,


        [EnumMember(Value = @"midi")]
        Midi = 3,


        [EnumMember(Value = @"openExternal")]
        OpenExternal = 4,


    }
}