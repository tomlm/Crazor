using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Crazor.Server.Teams
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Permission
    {

        [EnumMember(Value = @"identity")]
        Identity = 0,


        [EnumMember(Value = @"messageTeamMembers")]
        MessageTeamMembers = 1,


    }
}