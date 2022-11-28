// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;

namespace Crazor
{
    class UrlHelperProxy : IUrlHelper
    {
        private readonly IActionContextAccessor accessor;
        private readonly IUrlHelperFactory factory;
        private readonly Uri _uri;

        public UrlHelperProxy(IActionContextAccessor accessor, IUrlHelperFactory factory, IConfiguration configuration)
        {
            this.accessor = accessor;
            this.factory = factory;
            this._uri = configuration.GetValue<Uri>("HostUri");
        }

        public ActionContext ActionContext => UrlHelper.ActionContext;

        public string Action(UrlActionContext context) => UrlHelper.Action(context)!;

        public string Content(string? contentPath) => new Uri(this._uri, UrlHelper.Content(contentPath)!).AbsoluteUri;

        public bool IsLocalUrl(string? url) => UrlHelper.IsLocalUrl(url)!;

        public string Link(string? name, object? values) => UrlHelper.Link(name, values)!;

        public string RouteUrl(UrlRouteContext context) => UrlHelper.RouteUrl(context)!;

        private IUrlHelper UrlHelper => factory.GetUrlHelper(accessor.ActionContext!)!;
    }
}
