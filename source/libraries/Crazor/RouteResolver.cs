// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Attributes;
using Crazor.Interfaces;
using System.Reflection;

namespace Crazor
{
    public class RouteResolver : IRouteResolver
    {
        private Dictionary<string, List<RouteTemplate>> _routes = new Dictionary<string, List<RouteTemplate>>(StringComparer.OrdinalIgnoreCase);

        public RouteResolver()
        {
        }

        public bool IsRouteValid(CardRoute route)
        {
            return ResolveRoute(route, out var type);
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

        public void AddCardViewType(Type cardViewType)
        {
            if (cardViewType.Name == "CardViewBase`1" ||
                cardViewType.Name == "CardView" ||
                cardViewType.Name == "CardView`1" ||
                cardViewType.Name == "CardView`2")
            {
                // don't register base classes.
                return;
            }

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

                var routes = cardViewType.GetCustomAttributes<CardRouteAttribute>();
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
                string route = $"/{cardViewType.FullName.Substring(cardViewType.FullName.IndexOf("Cards")).Replace('.', '/')}";
                var routeAttribute = cardViewType.GetCustomAttribute<CardRouteAttribute>();
                if (routeAttribute != null)
                {
                    if (routeAttribute.Template.StartsWith('/'))
                    {
                        route = routeAttribute.Template;
                    }
                    else
                    {
                        cardRoute = CardRoute.Parse(route);
                        route = $"/Cards/{cardRoute.App}/{routeAttribute.Template}";
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
                var cardTemplate = parts.Skip(2).FirstOrDefault() ?? string.Empty;
                if (cardTemplate.ToLower() == "default")
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
