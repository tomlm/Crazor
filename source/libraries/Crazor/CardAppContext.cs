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
            CardTabModuleFactory cardTabModuleFactory,
            ICardViewFactory cardViewFactory,
            IRouteResolver routeResolver,
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
            RouteResolver = routeResolver;
        }


        public IConfiguration Configuration { get; set; }

        public IHttpContextAccessor HttpContextAccessor { get; }

        public IEncryptionProvider EncryptionProvider { get; set; }

        public IStorage Storage { get; set; }

        public IRazorViewEngine RazorEngine { get; set; }
        
        public ITempDataProvider TempDataProvider { get; set; }

        public IUrlHelper UrlHelper { get; set; }

        public CardAppFactory CardAppFactory { get; }


        public CardTabModuleFactory CardTabModuleFactory { get; }

        public ICardViewFactory CardViewFactory { get; set; }

        public IRouteResolver RouteResolver { get; set; }
    }
}
