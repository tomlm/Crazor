

using Crazor;
using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;

namespace AdaptiveCards
{
    /// <summary>
    /// Helper class for ActionSubmit => Task/fetch with commandId.
    /// </summary>
    public class AdaptiveShowTaskModuleAction : AdaptiveSubmitAction
    {
        private static JObject fetch = new JObject() { { "type", "task/fetch"} };

        public AdaptiveShowTaskModuleAction()
        {
            Data = new JObject();
        }

        public AdaptiveShowTaskModuleAction(string commandId)
        {
            Data = new JObject()
            {
                new JProperty("msteams", fetch),
                new JProperty("commandId", commandId)
            };
        }
    }
}
