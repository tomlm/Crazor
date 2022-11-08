﻿//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Crazor
{
    public partial class CardActivityHandler
    {
        protected async override Task<AdaptiveCardInvokeResponse> OnAdaptiveCardInvokeAsync(ITurnContext<IInvokeActivity> turnContext, AdaptiveCardInvokeValue invokeValue, CancellationToken cancellationToken)
        {
            CardApp cardApp = null;
            if (invokeValue.Action.Verb == Constants.LOADROUTE_VERB)
            {
                dynamic data = invokeValue.Action.Data;
                string path = (string)(data.path);
                var uri = new Uri(_configuration.GetValue<Uri>("HostUri"), path);
                cardApp = await LoadAppAsync(turnContext, uri, cancellationToken);
                cardApp.SharedId = cardApp.GetSharedId();
            }
            else
            {
                SessionData sessionData = await invokeValue.GetSessionDataFromInvokeAsync(_encryptionProvider, cancellationToken);
                cardApp = await this.LoadAppAsync(sessionData, (Activity)turnContext.Activity, cancellationToken);
            }
            var card = await cardApp.OnActionExecuteAsync(cancellationToken);
            await cardApp.SaveAppAsync(cancellationToken);
            return new AdaptiveCardInvokeResponse()
            {
                StatusCode = 200,
                Type = AdaptiveCard.ContentType,
                Value = card
            };
        }


        protected override Task OnSignInInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
            // we extract SSO tokens in Card Adapter where we have information about whether the channel is trusted
            // (if the channel is trusted, then we can cache the SSO token by the ABS key across different requests)
            // overriding this handler to avoid seeing this error in teams client
            // {"errorCode":1008, "message": "<BotError>Bot returned unsuccessful status code NotImplemented"}
            // in the future, this handler may also be needed to get result from Teams JS notifySuccess to signal web flow completion
            => Task.CompletedTask;
    }
}