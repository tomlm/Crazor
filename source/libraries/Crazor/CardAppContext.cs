// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Crazor
{
    public class CardAppContext
    {
        public CardAppContext(
            CardAppFactory cardAppFactory,
            CardViewFactory cardViewFactory,
            CardTabModuleFactory cardTabModuleFactory,
            RouteManager routeManager,
            IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor,
            IEncryptionProvider encryptionProvider, 
            IRazorViewEngine razorEngine, 
            IStorage storage, 
            ITempDataProvider tempDataProvider,
            IUrlHelper urlHelper)
        {
            Configuration = configuration;
            HttpContextAccessor = httpContextAccessor;
            EncryptionProvider = encryptionProvider;
            Storage = storage;
            RazorEngine = razorEngine;
            TempDataProvider = tempDataProvider;
            CardAppFactory = cardAppFactory;
            CardViewFactory = cardViewFactory;
            CardTabModuleFactory = cardTabModuleFactory;
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

        public CardAppFactory CardAppFactory { get; }

        public CardViewFactory CardViewFactory { get; set; }

        public CardTabModuleFactory CardTabModuleFactory { get; }

        public RouteManager RouteManager { get; set; }
    }
}
