using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Crazor.Teams
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ParametersInputType
    {

        [EnumMember(Value = @"text")]
        Text = 0,


        [EnumMember(Value = @"textarea")]
        Textarea = 1,


        [EnumMember(Value = @"number")]
        Number = 2,


        [EnumMember(Value = @"date")]
        Date = 3,


        [EnumMember(Value = @"time")]
        Time = 4,


        [EnumMember(Value = @"toggle")]
        Toggle = 5,


        [EnumMember(Value = @"choiceset")]
        Choiceset = 6,


    }
}