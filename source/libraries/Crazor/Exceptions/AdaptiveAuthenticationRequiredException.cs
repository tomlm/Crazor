using AdaptiveCards;
using System.Net;

namespace Crazor.Exceptions
{
    /// <summary>
    /// Authentication is required 
    /// </summary>
    public class AdaptiveAuthenticationRequiredException : Exception
    {
        public AdaptiveAuthenticationRequiredException(AdaptiveAuthentication authentication, HttpStatusCode statusCode) : base("Authentication is required for this card")
        {
            StatusCode = statusCode;
            Authentication = authentication;
        }

        public HttpStatusCode StatusCode { get; set; }

        public AdaptiveAuthentication Authentication { get; set; }
    }
}
