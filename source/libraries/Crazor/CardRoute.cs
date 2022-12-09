﻿// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
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
        public string Route { get; set; }

        /// <summary>
        /// App extracted from route
        /// </summary>
        public string App { get; set; }

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
            if (pathParts[0].ToLower() != "cards")
                throw new ArgumentException($"Unknown route {pathAndQuery}");

            result.App = pathParts[1].Trim();
            result.View = pathParts.Skip(2).FirstOrDefault() ?? "Default";
            result.Path = String.Join('/', local.Trim('/').Split('/').Skip(2).ToArray());
            result.RouteData = new JObject();
            result.QueryData = new JObject();
            if (result.Query != null)
            {
                var qp = QueryHelpers.ParseQuery(result.Query);
                foreach (var kv in qp)
                {
                    result.QueryData[kv.Key] = kv.Value.FirstOrDefault();
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

            // REMOVE this in 2023, deal with old sessiondata objects in session memory
            if (route == null && data.ContainsKey("_sessiondata"))
            {
                var parts = ((string)data["_sessiondata"]).Split('|');
                route = $"/Cards/{parts[0]}";
                data[Constants.ROUTE_KEY] = route;
                data[Constants.SESSION_KEY] = parts.Last();
                data.Remove("_sessiondata");
            }
            // REMOVE ^^^^ in 2023

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
