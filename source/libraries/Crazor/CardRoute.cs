using Crazor.Interfaces;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Crazor
{
    [DebuggerDisplay("{Route}")]
    public class CardRoute
    {
        /// <summary>
        /// Route path like /cards/Foo/path?q1=...
        /// </summary>
        public string Route { get; set; } = String.Empty;

        /// <summary>
        /// App extracted from route
        /// </summary>
        public string App { get; set; } = String.Empty;

        /// <summary>
        /// The view from the route
        /// </summary>
        public string? View { get; set; }

        /// <summary>
        /// just the path from route
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// ?Query from the route
        /// </summary>
        public string? Query { get; set; }

        /// <summary>
        /// Data extracted from the route
        /// </summary>
        public JObject RouteData { get; set; } = new JObject();

        /// <summary>
        /// Data extracted from the query
        /// </summary>
        public JObject QueryData { get; set; } = new JObject();

        /// <summary>
        /// SessionId (not stored in uri)
        /// </summary>
        public string? SessionId { get; set; }

        public static CardRoute Parse(string pathAndQuery)
        {
            ArgumentNullException.ThrowIfNull(pathAndQuery);
            var result = new CardRoute()
            {
                Route = pathAndQuery
            };

            var parts = pathAndQuery.Split('?');
            var local = parts[0];
            result.Query = parts.Skip(1).FirstOrDefault();
            var pathParts = local.Trim('/').Split('/');
            if (pathParts.Length < 2)
                throw new ArgumentException($"Unknown route {pathAndQuery}");

            if (pathParts[0].ToLower() != "cards")
                throw new ArgumentException($"Unknown route {pathAndQuery}");

            result.App = pathParts[1].Trim();
            result.View = pathParts.Skip(2).FirstOrDefault() ?? "Default";
            result.Path = String.Join('/', local.Trim('/').Split('/').Skip(2).ToArray());
            result.RouteData = new JObject();
            result.QueryData = new JObject();
            if (result.Query != null)
            {
                var qp = QueryString.Parse(result.Query.TrimStart('?'));
                foreach (var kv in qp)
                {
                    result.QueryData[kv.Name] = kv.Value;
                }
            }

            return result;
        }

        public static CardRoute FromUri(Uri uri)
        {
            return CardRoute.Parse(uri.PathAndQuery);
        }

        public static async Task<CardRoute> FromDataAsync(JObject data, IEncryptionProvider encryptionProvider, CancellationToken cancellationToken)
        {
            // Get session data from the invoke payload
            var route = (string?)data[Constants.ROUTE_KEY];

            ArgumentNullException.ThrowIfNull(route);
            var cardRoute = Parse(route);
            var sessionId = (string?)data[Constants.SESSION_KEY];
            if (sessionId != null)
            {
                cardRoute.SessionId = await encryptionProvider.DecryptAsync(sessionId, cancellationToken);
            }
            else
            {
                cardRoute.SessionId = Utils.GetNewId();
            }
            return cardRoute;
        }
    }
}
