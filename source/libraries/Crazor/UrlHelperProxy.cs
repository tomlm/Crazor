using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crazor
{
    class UrlHelperProxy : IUrlHelper
    {
        private readonly IActionContextAccessor accessor;
        private readonly IUrlHelperFactory factory;

        public UrlHelperProxy(IActionContextAccessor accessor, IUrlHelperFactory factory)
        {
            this.accessor = accessor;
            this.factory = factory;
        }
        public ActionContext ActionContext => UrlHelper.ActionContext;
        public string Action(UrlActionContext context) => UrlHelper.Action(context)!;
        public string Content(string? contentPath) => UrlHelper.Content(contentPath)!;
        public bool IsLocalUrl(string? url) => UrlHelper.IsLocalUrl(url)!;
        public string Link(string? name, object? values) => UrlHelper.Link(name, values)!;
        public string RouteUrl(UrlRouteContext context) => UrlHelper.RouteUrl(context)!;
        private IUrlHelper UrlHelper => factory.GetUrlHelper(accessor.ActionContext!)!;
    }
}
