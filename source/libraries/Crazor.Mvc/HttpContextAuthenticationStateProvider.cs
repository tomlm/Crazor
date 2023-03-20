// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Http;

namespace Crazor.Mvc
{
    public class HttpContextAuthenticationStateProvider : ServerAuthenticationStateProvider
    {
        public HttpContextAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor)
        {
            var claimsPrincipal = httpContextAccessor.HttpContext.User;
            if (claimsPrincipal != null)
            {
                this.SetAuthenticationState(Task.FromResult(new AuthenticationState(claimsPrincipal)));
            }
        }
    }
}
