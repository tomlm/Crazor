using Newtonsoft.Json;

namespace Crazor.AI.Recognizers
{

    public class Function
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("args")]
        public List<object> Args { get; set; } = new List<object>();

        public override string ToString()
        {
            return $"{Name}({string.Join(',', Args)})";
        }
    }
}
