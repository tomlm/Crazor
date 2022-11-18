//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System;

namespace Crazor
{
    public partial class CardActivityHandler
    {
        protected async override Task<AdaptiveCardInvokeResponse> OnAdaptiveCardInvokeAsync(ITurnContext<IInvokeActivity> turnContext, AdaptiveCardInvokeValue invokeValue, CancellationToken cancellationToken)
        {
            CardApp cardApp = null;
        
            var sessionId = turnContext.Activity?.Id ?? Utils.GetNewId();
            Activity activity = (Activity)turnContext.Activity!;
            if (invokeValue.Action.Verb == Constants.LOADROUTE_VERB)
            {
                dynamic data = invokeValue.Action.Data;
                var uri = new Uri(_configuration.GetValue<Uri>("HostUri"), (string)(data.path));
                cardApp = _cardAppFactory.CreateFromUri(uri, out var sharedId, out var view, out var path, out var query);
                activity = activity.CreateLoadRouteActivity(uri);
                await cardApp.LoadAppAsync(sharedId, null, activity, cancellationToken);
            }
            else
            {
                var sessionData = await invokeValue.GetSessionDataFromInvokeAsync(_encryptionProvider, cancellationToken);
                cardApp = _cardAppFactory.Create(sessionData.App);
                await cardApp.LoadAppAsync(sessionData, activity, cancellationToken);
            }

            await cardApp.OnActionExecuteAsync(cancellationToken);
            
            await cardApp.SaveAppAsync(cancellationToken);

            var card = await cardApp.RenderCardAsync(isPreview: false, cancellationToken);

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