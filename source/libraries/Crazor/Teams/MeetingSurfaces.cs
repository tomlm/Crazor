using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Crazor.Teams
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MeetingSurface
    {

        [EnumMember(Value = @"sidePanel")]
        SidePanel = 0,


        [EnumMember(Value = @"stage")]
        Stage = 1,


    }
}