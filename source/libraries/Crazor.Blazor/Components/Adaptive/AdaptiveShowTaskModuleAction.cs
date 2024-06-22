using Newtonsoft.Json.Linq;

namespace Crazor
{
    /// <summary>
    /// Helper class for ActionSubmit => Task/fetch with commandId.
    /// </summary>
    public class AdaptiveShowTaskModuleAction : AdaptiveSubmitAction
    {
        private static JObject fetch = new JObject() { { "type", "task/fetch" } };

        public AdaptiveShowTaskModuleAction()
        {
            Data = new JObject()
            {
                new JProperty("msteams", fetch)
            };
        }

        public string Route { get => ObjectPath.GetPathValue<string>(Data, "commandId"); set => ObjectPath.SetPathValue(Data, "commandId", value); }
    }
}
