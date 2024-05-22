using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Crazor.Server
{
    public partial class CardActivityHandler
    {
        protected override async Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
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
    }
}