// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Crazor.Test
{
    public class HttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }
    }
}
