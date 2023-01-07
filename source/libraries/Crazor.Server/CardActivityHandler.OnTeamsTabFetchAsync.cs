// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Diag = System.Diagnostics;
namespace Crazor.Server
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
            System.Diagnostics.Debug.WriteLine($"Starting OnTeamsTabFetchAsync() processing");
            CardTabModule tabModule = Context.CardTabModuleFactory.Create(tabRequest.TabEntityContext.TabEntityId);

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
                            Card = TransformCardNoRefresh(TransformActionExecuteToSubmit(card))
                        }).ToList()
                    }
                }
            };
        }
    }
}