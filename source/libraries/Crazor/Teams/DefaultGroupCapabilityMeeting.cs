﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Crazor.Teams
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DefaultGroupCapabilityMeeting
    {

        [EnumMember(Value = @"tab")]
        Tab = 0,


        [EnumMember(Value = @"bot")]
        Bot = 1,


        [EnumMember(Value = @"connector")]
        Connector = 2,


    }
}