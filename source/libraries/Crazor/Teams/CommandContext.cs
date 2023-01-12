using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Crazor.Teams
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CommandContext
    {

        [EnumMember(Value = @"compose")]
        Compose = 0,


        [EnumMember(Value = @"commandBox")]
        CommandBox = 1,


        [EnumMember(Value = @"message")]
        Message = 2,


    }
}