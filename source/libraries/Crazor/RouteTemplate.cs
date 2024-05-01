


using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Crazor
{
    [DebuggerDisplay("[{Order}] {Template}")]
    public class RouteTemplate
    {
        public Type Type { get; set; }

        public string App { get; set; }

        public string Route { get; set; }

        public string Template { get; set; }

        public int Order { get; set; }

        public bool Matched(string route, out JObject data)
        {
            data = new JObject();
            var templateParts = Template.TrimStart('/').Split('/');
            var parts = route.TrimStart('/').Split('/');
            if (templateParts.Length < parts.Length)
                return false;

            for (int i = 0; i < templateParts.Length; i++)
            {
                var part = (i < parts.Length) ? parts[i] : null;
                var templatePart = templateParts[i].Trim();
                if (part == null & templatePart != null)
                {
                    if (templatePart.StartsWith('{') && templatePart.EndsWith('}'))
                    {
                        var propertyName = templatePart.Trim('{', '}');
                        return propertyName.EndsWith("?") || templatePart.ToLower() == "default";
                    }
                    return false;
                }

                if (part?.ToLower() != templatePart.ToLower())
                {
                    string propertyName = null;
                    if (templatePart.StartsWith('{') && templatePart.EndsWith('}'))
                    {
                        propertyName = templatePart.Trim('{', '}');
                        if (String.IsNullOrEmpty(part) && !propertyName.EndsWith("?"))
                        {
                            return false;
                        }
                        data[propertyName.TrimEnd('?')] = part;
                    }
                    else if (String.IsNullOrEmpty(part))
                    {
                        return propertyName?.EndsWith('?') ?? false || templatePart.ToLower() == "default";
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
