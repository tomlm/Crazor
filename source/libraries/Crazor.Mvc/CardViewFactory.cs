// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Interfaces;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Http;
using System.Xml.Linq;
using Microsoft.AspNetCore.Routing;

namespace Crazor.Mvc
{
    /// <summary>
    /// CardViewFactory which instantiates cardviews for Mvc razor templates.
    /// </summary>
    public class CardViewFactory : ICardViewFactory
    {
        private readonly Dictionary<string, Type> _views = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private readonly IServiceProvider _serviceProvider;
        private readonly IRazorViewEngine _razorEngine;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IUrlHelper _urlHelper;

        public CardViewFactory(
            IServiceProvider serviceProvider,
            IRazorViewEngine engine,
            IHttpContextAccessor httpContextAccessor,
            ITempDataProvider tempDataProvider,
            IUrlHelper urlHelper)
        {
            _serviceProvider = serviceProvider;
            _razorEngine = engine;
            _httpContextAccessor = httpContextAccessor;
            _tempDataProvider = tempDataProvider;
            _urlHelper = urlHelper;
        }

        public void Add(string name, Type type)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(type);
            _views[name] = type;
        }

        public IEnumerable<string> GetNames() => _views.Keys.OrderBy(n => n);

        /// <summary>
        /// HasView
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns>true or false</returns>
        public bool HasView(string nameOrRoute)
        {
            if (nameOrRoute.StartsWith("/Cards"))
            {
                var cardRoute = CardRoute.Parse(nameOrRoute);
                var viewPath = Path.Combine("Cards", cardRoute.App, $"{cardRoute.View}.cshtml");
                var viewResult = _razorEngine.GetView(Environment.CurrentDirectory, viewPath, false);
                return (viewResult?.View != null);
            }
            return _views.ContainsKey(nameOrRoute);
        }

        public ICardView Create(CardRoute route)
        {
            IMvcCardView cardView;
            IView view;

            var viewPath = Path.Combine("Cards", route.App, $"{route.View}.cshtml");
            var viewResult = _razorEngine.GetView(Environment.CurrentDirectory, viewPath, false);

            view = viewResult?.View;
            if (view != null)
            {
                cardView = (IMvcCardView)((RazorView)viewResult.View).RazorPage;
                cardView.UrlHelper = _urlHelper;
                cardView.RazorView = viewResult.View;
            }
            else
            {
                throw new ArgumentNullException($"Unknown route {route.Route}");
            }

            PrepView((RazorPage)cardView, view);
            return cardView;
        }

        public ICardView Create(string typeName)
        {
            IMvcCardView cardView;
            IView view;

            if (!_views.TryGetValue(typeName, out var cardViewType))
            {
                throw new Exception($"{typeName} is not a known type");
            }
            cardView = (IMvcCardView)_serviceProvider.GetService(cardViewType);
            cardView.UrlHelper = _urlHelper;
            view = new ViewStub();

            PrepView((RazorPage)cardView, view);
            return cardView;
        }


        private void PrepView(RazorPage razorPage, IView view)
        {
            ActionContext actionContext = new ActionContext(_httpContextAccessor.HttpContext!, new Microsoft.AspNetCore.Routing.RouteData(), new ActionDescriptor());
            var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                 // Model = cardState.Model
            };

            var viewContext = new ViewContext(actionContext, view, viewDictionary, new TempDataDictionary(actionContext.HttpContext, _tempDataProvider), new StringWriter(), new HtmlHelperOptions());
            razorPage.ViewContext = viewContext;
        }


        private class ViewStub : IView
        {
            public string Path { get; set; } = string.Empty;

            public Task RenderAsync(ViewContext context)
            {
                return Task.CompletedTask;
            }
        }
    }
}