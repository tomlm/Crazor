using Azure.AI.OpenAI;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using YamlConverter;
using Newtonsoft.Json;
using System.Diagnostics;
using Crazor.AI.Recognizers;

namespace Crazor.AI
{
    public static class Utils
    {


        public static string GetLabels(JSchema input)
        {
            JObject labels = new JObject();
            foreach (var kv in input.Properties)
            {
                if (!kv.Value.ExtensionData.TryGetValue("label", out var label))
                {
                    label = kv.Key;
                }
                labels[kv.Key] = label;
            }

            return YamlConvert.SerializeObject(new { Labels = labels });
        }

      

    }
}
