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
                var input = turnContext.Activity.RemoveRecipientMention()?.Trim() ?? String.Empty;
                if (input.Contains("post"))
                {
                    var app = input.Replace("post", "").Trim();

                    if (_cardAppFactory.GetNames().Any(name => name.ToLower() == app.ToLower()))
                    {
                        var cardApp = await LoadAppAsync((Activity)turnContext.Activity, app, Utils.GetNewId(), Utils.GetNewId(), null, cancellationToken);

                        await cardApp.OnActionExecuteAsync(cancellationToken);

                        await cardApp.SaveAppAsync(cancellationToken);

                        var card = await cardApp.RenderCardAsync(isPreview: false, cancellationToken);

                        var response = Activity.CreateMessageActivity();
                        response.Attachments.Add(new Attachment(AdaptiveCard.ContentType, content: card));

                        await turnContext.SendActivityAsync(response, cancellationToken);
                    }
                }
            }
        }

    }
}