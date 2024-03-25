using System.Reflection;
using YamlConverter;
using Newtonsoft.Json;

namespace Crazor.AI
{
    public static class Extensions
    {

        public static async Task<T> GetYamlOrJsonAsync<T>(this HttpClient httpClient, string url, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var response = await httpClient.GetAsync(url))
            {
                var content = await response.Content.ReadAsStringAsync();
                if (response.Content.Headers.ContentType.MediaType == "text/yaml")
                    return YamlConvert.DeserializeObject<T>(content);
                else if (response.Content.Headers.ContentType.MediaType == "application/json")
                    return JsonConvert.DeserializeObject<T>(content);
                else
                {
                    if (content.StartsWith("{") && content.EndsWith("}"))
                        return YamlConvert.DeserializeObject<T>(content);
                    else
                        return JsonConvert.DeserializeObject<T>(content);
                }
            }
        }

        public static string LoadResource(this Assembly assembly, string resource)
        {
            if (assembly.GetManifestResourceNames().Any(name => name == resource))
            {
                using (var stream = assembly.GetManifestResourceStream(resource))
                {
                    using (var tr = new StreamReader(stream))
                    {
                        return tr.ReadToEnd();
                    }
                }
            }
            return null;
        }
    }
}
