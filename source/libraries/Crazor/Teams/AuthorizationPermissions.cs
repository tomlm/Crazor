using Newtonsoft.Json;

namespace Crazor.Teams
{
    public class AuthorizationPermissions
    {
        /// <summary>
        /// Permissions that guard data access on a resource instance level.
        /// </summary>
        [JsonProperty("resourceSpecific", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<ResourceSpecific> ResourceSpecific { get; set; }


    }
}