// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Crazor.Blazor
{

    public class RouteResolver : IRouteResolver
    {
        private Dictionary<string, List<RouteTemplate>> _routes = new Dictionary<string, List<RouteTemplate>>(StringComparer.OrdinalIgnoreCase);

        public RouteResolver()
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
                        // code based view - use full name of the type.
                        route.View = type.FullName;
                        return true;
                    }
                }
            }
            return false;
        }

        public void AddCardViewType(Type cardViewType)
        {
            CardRoute cardRoute;
            List<RouteTemplate> list;

            string route = $"/{cardViewType.Namespace.Substring(cardViewType.Namespace.IndexOf("Cards")).Replace('.', '/')}";
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
            _routes[cardRoute.App] = list.OrderBy(o => o.Order).ThenByDescending(o => o.Template).ToList();
        }
    }
}
