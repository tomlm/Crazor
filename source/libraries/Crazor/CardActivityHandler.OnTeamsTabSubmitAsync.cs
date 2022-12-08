// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

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
        protected override async Task<TabResponse> OnTeamsTabSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TabSubmit tabSubmit, CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine($"Starting OnTeamsTabSubmitAsync() processing");
            CardTabModule tabModule = Context.CardTabModuleFactory.Create(tabSubmit.TabEntityContext.TabEntityId);

            AdaptiveCardInvokeValue invokeValue = Utils.TransfromSubmitDataToExecuteAction(JObject.FromObject(tabSubmit.Data));
            var cards = await tabModule.OnTabSubmitAsync(turnContext, tabSubmit, invokeValue, cancellationToken);

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
