// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Crazor.Mvc
{
    /// <summary>
    /// CardViewFactory which instantiates cardviews for Mvc razor templates.
    /// </summary>
    public class MvcCardViewFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IRazorViewEngine _razorEngine;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITempDataProvider _tempDataProvider;

        public MvcCardViewFactory(
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

        public ICardView Create(Type cardViewType)
        {
            IMvcCardView cardView = null;
            IView view = null;

            // if it is a CSHTML file it will have Cards_ in the name
            var parts = cardViewType.FullName.Split('.');
            if (parts.Any(p => p.ToLower().StartsWith("cards_")))
            {
                parts = parts.Last().Split('_');
                parts[parts.Length - 1] = parts[parts.Length - 1] + ".cshtml";
                var viewPath = Path.Combine(parts.ToArray());
                var viewResult = _razorEngine.GetView(Environment.CurrentDirectory, viewPath, false);

                view = viewResult?.View;
                if (view != null)
                {
                    cardView = (IMvcCardView)((RazorView)viewResult.View).RazorPage;
                    cardView.RazorView = viewResult.View;
                }
            }
            ArgumentNullException.ThrowIfNull(cardView);

            ActionContext actionContext = new ActionContext(_httpContextAccessor.HttpContext!, new Microsoft.AspNetCore.Routing.RouteData(), new ActionDescriptor());
            var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                // Model = cardState.Model
            };

            var viewContext = new ViewContext(actionContext, view, viewDictionary, new TempDataDictionary(actionContext.HttpContext, _tempDataProvider), new StringWriter(), new HtmlHelperOptions());
            ((RazorPage)cardView).ViewContext = viewContext;
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