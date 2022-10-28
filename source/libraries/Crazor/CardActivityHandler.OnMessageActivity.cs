//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Crazor
{
    public partial class CardActivityHandler
    {
        /// <summary>
        /// Process user typed messages
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            IMessageActivity message = turnContext.Activity.AsMessageActivity();
            if (message != null)
            {
                var app = turnContext.Activity.RemoveRecipientMention()?.Trim() ?? String.Empty;
                if (app.ToLower().EndsWith("app"))
                {
                    var cardApp = await LoadAppAsync(turnContext, app, Utils.GetNewId(), Utils.GetNewId(), null, cancellationToken);
                    var card = await cardApp.OnActionExecuteAsync(cancellationToken);
                    await cardApp.SaveAppAsync(cancellationToken);
                    var response = Activity.CreateMessageActivity();
                    response.Attachments.Add(new Attachment(AdaptiveCard.ContentType, content: card));
                    await turnContext.SendActivityAsync(response, cancellationToken);
                }
            }
        }

    }
}