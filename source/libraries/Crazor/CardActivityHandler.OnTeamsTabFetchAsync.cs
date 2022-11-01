//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Diag = System.Diagnostics;
namespace Crazor
{
    public partial class CardActivityHandler
    {
        /// <summary>
        /// Handle Fetch Task request
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<TabResponse> OnTeamsTabFetchAsync(ITurnContext<IInvokeActivity> turnContext, TabRequest tabRequest, CancellationToken cancellationToken)
        {
            Diag.Debug.WriteLine($"Activity id:{turnContext.Activity.Id}");
            Diag.Debug.WriteLine($"Conversation id:{turnContext.Activity.Conversation.Id}");
            _logger!.LogInformation($"Starting OnTeamsTabFetchAsync() processing");
            CardTabModule tabModule = tabRequest.TabEntityContext.TabEntityId.StartsWith("/Cards") 
                ? new SingleCardTabModule(_serviceProvider, tabRequest.TabEntityContext.TabEntityId)
                : _tabs.GetRequiredByName(tabRequest.TabEntityContext.TabEntityId);

            var cards = await tabModule.OnTabFetchAsync(turnContext, tabRequest, cancellationToken);

            return new TabResponse()
            {
                Tab = new TabResponsePayload()
                {
                    Type = "continue",
                    Value = new TabResponseCards()
                    {
                        Cards = cards.Select(card => new TabResponseCard()
                        {
                            Card = TransformActionExecuteToSubmit(card)
                        }).ToList()
                    }
                }
            };
        }
    }
}