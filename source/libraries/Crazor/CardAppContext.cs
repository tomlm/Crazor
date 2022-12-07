// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Neleus.DependencyInjection.Extensions;

namespace Crazor
{
    public class CardAppContext
    {
        public CardAppContext(
            IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor,
            IEncryptionProvider encryptionProvider, 
            IStorage storage, 
            IRazorViewEngine razorEngine, 
            ITempDataProvider tempDataProvider,
            CardAppFactory cardAppFactory,
            IServiceByNameFactory<ICardView> cardViewFactory, 
            IUrlHelper urlHelper, 
            RouteManager routeManager)
        {
            Configuration = configuration;
            HttpContextAccessor = httpContextAccessor;
            EncryptionProvider = encryptionProvider;
            Storage = storage;
            RazorEngine = razorEngine;
            TempDataProvider = tempDataProvider;
            CardAppFactory = cardAppFactory;
            CardViewFactory = cardViewFactory;
            UrlHelper = urlHelper;
            RouteManager = routeManager;
        }


        public IConfiguration Configuration { get; set; }

        public IHttpContextAccessor HttpContextAccessor { get; }

        public IEncryptionProvider EncryptionProvider { get; set; }

        public IStorage Storage { get; set; }

        public IRazorViewEngine RazorEngine { get; set; }

        public ITempDataProvider TempDataProvider { get; set; }

        public IUrlHelper UrlHelper { get; set; }

        public IServiceByNameFactory<ICardView> CardViewFactory { get; set; }

        public CardAppFactory CardAppFactory { get; }

        public RouteManager RouteManager { get; set; }
    }
}
