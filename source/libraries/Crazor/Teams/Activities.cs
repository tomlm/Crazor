using Newtonsoft.Json;

namespace Crazor.Teams
{
    public class Activities
    {
        /// <summary>
        /// Specify the types of activites that your app can post to a users activity feed
        /// </summary>
        [JsonProperty("activityTypes", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<ActivityType> ActivityTypes { get; set; }


    }
}