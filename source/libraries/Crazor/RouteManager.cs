// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Crazor
{
    [DebuggerDisplay("[{Order}] {Template}")]
    internal class RouteTemplate
    {
        internal Type Type { get; set; }

        internal string App { get; set; }

        internal string Route { get; set; }

        internal string Template { get; set; }

        internal int Order { get; set; }

        internal bool Matched(string route, out JObject data)
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

    public class RouteManager
    {
        private Dictionary<string, List<RouteTemplate>> _routes = new Dictionary<string, List<RouteTemplate>>(StringComparer.OrdinalIgnoreCase);

        public RouteManager()
        {
        }

        public bool ResolveRoute(CardRoute route, out Type? type)
        {
            type = null;
            if (_routes.TryGetValue(route.App, out var routeTemplates))
            {
                var parts = route.Path.Split('/');

                foreach (var routeTemplate in routeTemplates)
                {
                    if (routeTemplate.Matched(route.Path, out var data))
                    {
                        foreach (var property in data.Properties())
                        {
                            route.RouteData[property.Name] = property.Value;
                        }
                        type = routeTemplate.Type;
                        if (type.Name.Contains('_'))
                        {
                            route.View = type.Name.Split('_').Last();
                        }
                        else
                        {
                            // code based view - use full name of the type.
                            route.View = type.FullName;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public void Add(Type cardViewType)
        {
            CardRoute cardRoute;
            List<RouteTemplate> list;
            if (cardViewType.Name.Contains("_"))
            {
                cardRoute = CardRoute.Parse(cardViewType.Name.Replace("_", "/"));

                var parts = cardViewType.Name.Split('_').ToList();
                var cardTemplate = parts.Last();
                if (cardTemplate.ToLower() == "Default")
                    cardTemplate = String.Empty;

                int order = 0;

                var path = $"{String.Join('/', parts.Take(2))}";
                if (!_routes.TryGetValue(cardRoute.App, out list))
                {
                    list = new List<RouteTemplate>();
                    _routes.Add(cardRoute.App, list);
                }

                var routes = cardViewType.GetCustomAttributes<RouteAttribute>();
                if (!routes.Any())
                {
                    list.Add(new RouteTemplate
                    {
                        Type = cardViewType,
                        App = cardRoute.App,
                        Route = path,
                        Template = cardTemplate,
                        Order = order,
                    });
                }
                else
                {
                    foreach (var routeAttribute in routes)
                    {
                        order = routeAttribute.Order;
                        cardTemplate = routeAttribute.Template;

                        list.Add(new RouteTemplate
                        {
                            Type = cardViewType,
                            App = cardRoute.App,
                            Route = path,
                            Template = cardTemplate,
                            Order = order,
                        });
                    }
                }
            }

            else
            {
                string route = $"/{cardViewType.Namespace.Substring(cardViewType.Namespace.IndexOf("Cards")).Replace('.','/')}";
                var routeAttribute = cardViewType.GetCustomAttribute<RouteAttribute>();
                if (routeAttribute != null)
                {
                    if (routeAttribute.Template.StartsWith('/'))
                    {
                        route = routeAttribute.Template;
                    }
                    else
                    {
                        route = $"{route}/{routeAttribute.Template}";
                    }
                }
                cardRoute = CardRoute.Parse(route);
                int order = 0;

                if (!_routes.TryGetValue(cardRoute.App, out list))
                {
                    list = new List<RouteTemplate>();
                    _routes.Add(cardRoute.App, list);
                }
                var parts = route.TrimStart('/').Split('/');
                var cardTemplate = parts.Skip(2).SingleOrDefault() ?? string.Empty;
                if (cardTemplate.ToLower() == "Default")
                    cardTemplate = String.Empty;

                list.Add(new RouteTemplate
                {
                    Type = cardViewType,
                    App = cardRoute.App,
                    Route = String.Join('/', parts.Take(2)),
                    Template = String.Join('/', parts.Skip(2)),
                    Order = order,
                });
            }
            _routes[cardRoute.App] = list.OrderBy(o => o.Order).ThenByDescending(o => o.Template).ToList();
        }
    }
}
