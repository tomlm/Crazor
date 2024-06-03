using AdaptiveCards;
using Crazor.Exceptions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Crazor.Server
{
    public partial class CardActivityHandler
    {
        protected override async Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                switch (turnContext.Activity.Name)
                {
                    case "application/search":
                        var searchInvoke = JObject.FromObject(turnContext.Activity.Value).ToObject<SearchInvoke>();
                        return CreateInvokeResponse(await OnSearchInvokeAsync(turnContext, searchInvoke!, cancellationToken).ConfigureAwait(false));

                    default:
                        return await base.OnInvokeActivityAsync(turnContext, cancellationToken);
                }
            }
            catch (AdaptiveAuthenticationRequiredException authException)
            {
                // return required login request.
                return ActivityHandler.CreateInvokeResponse(new AdaptiveCardInvokeResponse()
                {
                    Type = "application/vnd.microsoft.activity.loginRequest",
                    StatusCode = (int)authException.StatusCode,
                    Value = authException.Authentication
                });
            }
        }

        /*
         * {
"statusCode": 401,
"type": "application/vnd.microsoft.activity.loginRequest",
"value": {
"text": "Please sign-in",
"connectionName": "<configured-connection-name>",
"tokenExchangeResource": {
  "id": "<unique-indentifier>",
  "uri": "<application-or-resource-identifier>",
  "providerId": "<optional-provider-identifier>"
},
"buttons": [
  {
  "title": "Sign-In",
     "text": "Sign-In",
     "type": "signin",
     "value": "<sign-in-URL>"
  }
]
}
}*/
    }
}