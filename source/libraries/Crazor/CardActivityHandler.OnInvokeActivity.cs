//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using Crazor.Interfaces;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;

namespace Crazor
{
    public partial class CardActivityHandler
    {
        protected override async Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            switch (turnContext.Activity.Name)
            {
                case "application/search":
                    var searchInvoke = JObject.FromObject(turnContext.Activity.Value).ToObject<SearchInvoke>();
                    return CreateInvokeResponse(await OnSearchInvokeAsync(turnContext, searchInvoke, cancellationToken).ConfigureAwait(false));

                default:
                    return await base.OnInvokeActivityAsync(turnContext, cancellationToken);
            }
        }
    }
}