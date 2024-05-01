


using Microsoft.AspNetCore.Http;

namespace Crazor.Test
{
    public class HttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }
    }
}
