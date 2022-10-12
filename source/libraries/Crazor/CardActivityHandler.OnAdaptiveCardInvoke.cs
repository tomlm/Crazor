//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Crazor.Interfaces;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Crazor
{
    public partial class CardActivityHandler
    {
        protected async override Task<AdaptiveCardInvokeResponse> OnAdaptiveCardInvokeAsync(ITurnContext<IInvokeActivity> turnContext, AdaptiveCardInvokeValue invokeValue, CancellationToken cancellationToken)
        {
            SessionData sessionData = await GetSessionDataFromInvokeAsync(invokeValue, cancellationToken);

            // Create card from sessiondata
            var cardApp = await this.LoadAppAsync(sessionData, (Activity)turnContext.Activity, cancellationToken);

            // process Action
            var invokeResponse = await cardApp.OnActionExecuteAsync(cancellationToken);

            AdaptiveCard adaptiveCard = (AdaptiveCard)invokeResponse.Value;
            if (turnContext.Activity.ChannelId == Channels.Msteams)
            {
                // update refresh members list.
                await SetTeamsRefreshUserIds(adaptiveCard, turnContext, cancellationToken);
            }

            await cardApp.SaveAppAsync(cancellationToken);
            return invokeResponse;
        }


        protected async Task<SessionData> GetSessionDataFromInvokeAsync(AdaptiveCardInvokeValue invokeValue, CancellationToken cancellationToken)
        {
            // Get session data from the invoke payload
            IEncryptionProvider encryptionProvider = _serviceProvider.GetRequiredService<IEncryptionProvider>();
            var data = await encryptionProvider.DecryptAsync((string)((dynamic)invokeValue.Action.Data)._sessiondata, cancellationToken);
            var sessionData = SessionData.FromString(data);
            return sessionData;
        }

        private async Task SetTeamsRefreshUserIds(AdaptiveCard adaptiveCard, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
            try
            {
                var teamsMembers = await connectorClient.Conversations.GetConversationPagedMembersAsync(turnContext.Activity.Conversation.Id, 60,
                    cancellationToken: cancellationToken);
                adaptiveCard.Refresh.UserIds =
                    teamsMembers.Members.Select(member => $"8:orgid:{member.AadObjectId}").ToList();
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "Unable to set Refresh UserIds");
                // await _errors.HandleAsync(e, cancellationToken);
            }
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