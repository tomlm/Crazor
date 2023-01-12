using Newtonsoft.Json;

namespace Crazor.Teams
{
    public class Authorization
    {
        /// <summary>
        /// List of permissions that the app needs to function.
        /// </summary>
        [JsonProperty("permissions", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public AuthorizationPermissions Permissions { get; set; }


    }
}