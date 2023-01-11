using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Crazor.Server.Teams
{
    public class Command
    {
        /// <summary>
        /// Id of the command.
        /// </summary>
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        /// <summary>
        /// Type of the command
        /// </summary>
        [JsonProperty("type", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public CommandType Type { get; set; } = CommandType.Action;

        /// <summary>
        /// Context where the command would apply
        /// </summary>
        [JsonProperty("context", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<CommandContext> Context { get; set; }

        /// <summary>
        /// Title of the command.
        /// </summary>
        [JsonProperty("title", Required = Required.Always)]
        public string Title { get; set; }

        /// <summary>
        /// Description of the command.
        /// </summary>
        [JsonProperty("description", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        /// <summary>
        /// A boolean value that indicates if the command should be run once initially with no parameter.
        /// </summary>
        [JsonProperty("initialRun", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool InitialRun { get; set; } = false;

        /// <summary>
        /// A boolean value that indicates if it should fetch task module dynamically
        /// </summary>
        [JsonProperty("fetchTask", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool FetchTask { get; set; } = true;

        [JsonProperty("parameters", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<Parameter> Parameters { get; set; }

        [JsonProperty("taskInfo", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public TaskInfo TaskInfo { get; set; }


    }
}