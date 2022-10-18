using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Crazor
{
    public class LoadRouteModel
    {
        public string? View { get; set; }

        public string? Path { get; set; }

        public JObject GetDataForRoute(RouteAttribute route)
        {
            JObject result = new JObject();
            result["view"] = this.View;
            result["path"] = this.Path;

            var dataParts = Path!.Split('?');
            var dataPathParts = dataParts.First().Split('/');
            var dataQuery = dataParts.Skip(1).FirstOrDefault();
            var templateParts = route.Template.Split('?');
            int i = 0;
            foreach (var fragment in templateParts[0].Split('/'))
            {
                if (fragment.StartsWith('{') && fragment.EndsWith('}'))
                {
                    var name = fragment.TrimStart('{').TrimEnd('}', '?');
                    result[name] = dataPathParts[i];
                }
                i++;
            }
            return result;
        }
    }
}
