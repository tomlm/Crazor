using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Crazor.Server.Teams
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ConfigurableProperties
    {

        [EnumMember(Value = @"name")]
        Name = 0,


        [EnumMember(Value = @"shortDescription")]
        ShortDescription = 1,


        [EnumMember(Value = @"longDescription")]
        LongDescription = 2,


        [EnumMember(Value = @"smallImageUrl")]
        SmallImageUrl = 3,


        [EnumMember(Value = @"largeImageUrl")]
        LargeImageUrl = 4,


        [EnumMember(Value = @"accentColor")]
        AccentColor = 5,


        [EnumMember(Value = @"developerUrl")]
        DeveloperUrl = 6,


        [EnumMember(Value = @"privacyUrl")]
        PrivacyUrl = 7,


        [EnumMember(Value = @"termsOfUseUrl")]
        TermsOfUseUrl = 8,


    }
}