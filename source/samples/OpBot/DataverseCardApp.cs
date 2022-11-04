using Crazor;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace OpBot
{
    public abstract class DataverseCardApp : CardApp
    {
        protected DataverseCardApp(IServiceProvider services) : base(services)
        {
        }

       
        public static bool TryGetHeaders(IServiceProvider provider, out Uri? uri, out string? token)
            => TryGetHeaders(provider.GetRequiredService<IActionContextAccessor>().ActionContext!.HttpContext, out uri, out token);

        private static readonly Uri DefaultUri = new Uri("https://org829366eb.crm.dynamics.com/api/data/v9.2/");

        public static bool TryGetHeaders(HttpContext context, out Uri? uri, out string? token)
        {
            uri = null;
            token = null;

            var headers = context.Request.Headers;
            {
                if (!headers.TryGetValue("X-DV-Token", out var values))
                {
                    return false;
                }

                if (values.Count == 0)
                {
                    return false;
                }

                token = values[0];
                if (token.StartsWith("Bearer "))
                {
                    token = token[7..];
                }
            }

            {
                if (!headers.TryGetValue("X-DV-Url", out var values))
                {
                    uri = DefaultUri;
                    return true;
                }

                if (values.Count == 0)
                {
                    uri = DefaultUri;
                    return true;
                }

                var url = values[0];
                if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                {
                    uri = DefaultUri;
                    return true;
                }
            }

            if (string.IsNullOrWhiteSpace(uri.AbsolutePath))
            {
                var builder = new UriBuilder(uri)
                {
                    Path = DefaultUri.AbsolutePath,
                };

                uri = builder.Uri;
            }

            return true;
        }

        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
        };

        public static async Task<T?> SendAsync<T>(HttpMethod method, string path, Uri uri, string token, object? body = null)
        {
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(method, new Uri(uri, path));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (body != null)
            {
                var json = JsonConvert.SerializeObject(body, Settings);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using var response = await client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception error)
            {
                var text = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    throw new Exception(text, error);
                }

                throw;
            }

            if (typeof(Uri).IsAssignableFrom(typeof(T)))
            {
                return (T?)(object?)response.Headers.Location;
            }

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return default;
            }

            {
                var json = await response.Content.ReadAsStringAsync();
                var item = JsonConvert.DeserializeObject<T>(json, Settings);
                return item;
            }
        }

        public override async Task LoadAppAsync(string? sharedId, string? sessionId, Activity activity, CancellationToken cancellationToken)
        {
            await base.LoadAppAsync(sharedId, sessionId, activity, cancellationToken);
        }

        public async Task<T?> GetResponseAsync<T>(HttpMethod method, string urlPath)
        {
            if (TryGetHeaders(Services, out var uri, out var token))
            {
                return await SendAsync<T>(method, urlPath, uri!, token!);
            }
            else
            {
                return default;
            }
        }
    }
}
