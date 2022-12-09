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

        public CardViewFactory(
            IServiceProvider serviceProvider,
            IRazorViewEngine engine,
            IHttpContextAccessor httpContextAccessor,
            ITempDataProvider tempDataProvider)
        {
            _serviceProvider = serviceProvider;
            _razorEngine = engine;
            _httpContextAccessor = httpContextAccessor;
            _tempDataProvider = tempDataProvider;
        }

        public void Add(string name, Type type)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(type);
            _views[name] = type;
        }

        public IEnumerable<string> GetNames() => _views.Keys.OrderBy(n => n);

        public ICardView Create(CardRoute route)
        {
            CardView cardView;
            IView view;

            if (_views.TryGetValue(route.View, out var cardViewType))
            {
                cardView = (CardView)_serviceProvider.GetService(cardViewType);
                view = new ViewStub();
            }
            else
            {
                var viewPath = Path.Combine("Cards", route.App, route.View);
                var viewResult = _razorEngine.GetView(Environment.CurrentDirectory, viewPath, false);

                view = viewResult?.View;
                if (view != null)
                {
                    cardView = (CardView)((RazorView)viewResult.View).RazorPage;
                    ((CardView)cardView).RazorView = viewResult.View;
                }
                else
                {
                    throw new ArgumentNullException($"Unknown route {route.Route}");
                }
            }

            ActionContext actionContext;
            actionContext = new ActionContext(_httpContextAccessor.HttpContext!, new Microsoft.AspNetCore.Routing.RouteData(), new ActionDescriptor());
            var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                // Model = cardState.Model
            };

            var viewContext = new ViewContext(actionContext, view, viewDictionary, new TempDataDictionary(actionContext.HttpContext, _tempDataProvider), new StringWriter(), new HtmlHelperOptions());
            cardView.ViewContext = viewContext;

            return cardView;
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