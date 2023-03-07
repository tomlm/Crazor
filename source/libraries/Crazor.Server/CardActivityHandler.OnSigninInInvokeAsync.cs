// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Crazor.Server
{
    public partial class CardActivityHandler
    {

        /// <summary>
        /// Invoked when a signIn invoke activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override Task OnSignInInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            return base.OnSignInInvokeAsync(turnContext, cancellationToken);
        }

        // NOTE: OnTeamsSigninVerifyStateAsync is only called by default for OnSignInInvokeAsync(). There is no reason to define that I can determine, since we are
        // defining OnSigninInvokeAsync().
        // protected override Task OnTeamsSigninVerifyStateAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)

//        /// <summary>
//        /// Invoked when a <c>tokens/response</c> event is received when the base behavior of
//        /// <see cref="OnEventActivityAsync(ITurnContext{IEventActivity}, CancellationToken)"/> is used.
//        /// If using an <c>OAuthPrompt</c>, override this method to forward this <see cref="Activity"/> to the current dialog.
//        /// By default, this method does nothing.
//        /// </summary>
//        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
//        /// <param name="cancellationToken">A cancellation token that can be used by other objects
//        /// or threads to receive notice of cancellation.</param>
//        /// <returns>A task that represents the work queued to execute.</returns>
//        /// <remarks>
//        /// When the <see cref="OnEventActivityAsync(ITurnContext{IEventActivity}, CancellationToken)"/>
//        /// method receives an event with a <see cref="IEventActivity.Name"/> of `tokens/response`,
//        /// it calls this method.
//        ///
//        /// If your bot uses the <c>OAuthPrompt</c>, forward the incoming <see cref="Activity"/> to
//        /// the current dialog.
//        /// </remarks>
//        /// <seealso cref="OnEventActivityAsync(ITurnContext{IEventActivity}, CancellationToken)"/>
//        /// <seealso cref="OnEventAsync(ITurnContext{IEventActivity}, CancellationToken)"/>
//        protected override async Task OnTokenResponseEventAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
//        {
//            // Cribbed FROM SSOTokenExchangeMiddleware.cs

//            // If the TokenExchange is NOT successful, the response will have already been sent by ExchangedTokenAsync
//            if (!await ExchangeTokenAsync(turnContext, cancellationToken))
//            {
//                return;
//            }

//            // Only one token exchange should proceed from here. Deduplication is performed second because in the case
//            // of failure due to consent required, every caller needs to receive the 
//            if (!await DeduplicatedTokenExchangeIdAsync(turnContext, cancellationToken).ConfigureAwait(false))
//            {
//                // If the token is not exchangeable, do not process this activity further.
//                return;
//            }
//        }

//        private async Task<bool> ExchangeTokenAsync(ITurnContext turnContext, CancellationToken cancellationToken)
//        {
//            TokenResponse tokenExchangeResponse = null;
//            var tokenExchangeInvokeRequest = ((JObject)turnContext.Activity.Value)?.ToObject<TokenExchangeInvokeRequest>()!;

//            try
//            {
//                var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
//                if (userTokenClient != null)
//                {
//                    tokenExchangeResponse = await userTokenClient.ExchangeTokenAsync(
//                        turnContext.Activity.From.Id,
//                        tokenExchangeInvokeRequest.ConnectionName,
//                        turnContext.Activity.ChannelId,
//                        new TokenExchangeRequest { Token = tokenExchangeInvokeRequest.Token },
//                        cancellationToken).ConfigureAwait(false);
//                }
//                else
//                {
//                    throw new NotSupportedException("Token Exchange is not supported by the current adapter.");
//                }
//            }
//#pragma warning disable CA1031 // Do not catch general exception types (ignoring, see comment below)
//            catch
//#pragma warning restore CA1031 // Do not catch general exception types
//            {
//                // Ignore Exceptions
//                // If token exchange failed for any reason, tokenExchangeResponse above stays null,
//                // and hence we send back a failure invoke response to the caller.
//            }

//            if (string.IsNullOrEmpty(tokenExchangeResponse?.Token))
//            {
//                // The token could not be exchanged (which could be due to a consent requirement)
//                // Notify the sender that PreconditionFailed so they can respond accordingly.

//                var invokeResponse = new TokenExchangeInvokeResponse
//                {
//                    Id = tokenExchangeInvokeRequest.Id,
//                    ConnectionName = tokenExchangeInvokeRequest.ConnectionName,
//                    FailureDetail = "The bot is unable to exchange token. Proceed with regular login.",
//                };

//                await SendInvokeResponseAsync(turnContext, invokeResponse, HttpStatusCode.PreconditionFailed, cancellationToken).ConfigureAwait(false);

//                return false;
//            }

//            return true;
//        }


//        private async Task<bool> DeduplicatedTokenExchangeIdAsync(ITurnContext turnContext, CancellationToken cancellationToken)
//        {
//            var storage = turnContext.TurnState.Get<IStorage>();

//            // Create a StoreItem with Etag of the unique 'signin/tokenExchange' request
//            var storeItem = new TokenStoreItem
//            {
//                ETag = (turnContext.Activity.Value as JObject).Value<string>("id")
//            };

//            var storeItems = new Dictionary<string, object> { { TokenStoreItem.GetStorageKey(turnContext), storeItem } };
//            try
//            {
//                // Writing the IStoreItem with ETag of unique id will succeed only once
//                await storage.WriteAsync(storeItems, cancellationToken).ConfigureAwait(false);
//            }
//            catch (Exception ex)

//                // Memory storage throws a generic exception with a Message of 'Etag conflict. [other error info]'
//                // CosmosDbPartitionedStorage throws: ex.Message.Contains("pre-condition is not met")
//                when (ex.Message.StartsWith("Etag conflict", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("pre-condition is not met"))
//            {
//                // Do NOT proceed processing this message, some other thread or machine already has processed it.

//                // Send 200 invoke response.
//                await SendInvokeResponseAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
//                return false;
//            }

//            return true;
//        }

//        private async Task SendInvokeResponseAsync(ITurnContext turnContext, object body = null, HttpStatusCode httpStatusCode = HttpStatusCode.OK, CancellationToken cancellationToken = default)
//        {
//            await turnContext.SendActivityAsync(new Activity { Type = ActivityTypesEx.InvokeResponse, Value = new InvokeResponse { Status = (int)httpStatusCode, Body = body, } }, cancellationToken);
//        }

//        internal class TokenStoreItem : IStoreItem
//        {
//            public string ETag { get; set; }

//            public static string GetStorageKey(ITurnContext turnContext)
//            {
//                var activity = turnContext.Activity;
//                var channelId = activity.ChannelId ?? throw new InvalidOperationException("invalid activity-missing channelId");
//                var conversationId = activity.Conversation?.Id ?? throw new InvalidOperationException("invalid activity-missing Conversation.Id");

//                var value = activity.Value as JObject;
//                if (value == null || !value.ContainsKey("id"))
//                {
//                    throw new InvalidOperationException("Invalid signin/tokenExchange. Missing activity.Value.Id.");
//                }

//                return $"{channelId}/{conversationId}/{value.Value<string>("id")}";
//            }
//        }

//        /// <summary>
//        /// Authentication success.
//        /// AuthToken or state is present. Verify token/state in invoke payload and return AC response
//        /// </summary>
//        /// <param name="authentication">authToken are absent, handle verb</param>
//        /// <param name="state">state are absent, handle verb</param>
//        /// <param name="turnContext">The context for the current turn.</param>
//        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
//        /// <param name="isBasicRefresh">Refresh type</param>
//        /// <param name="fileName">AdaptiveCardResponse.json</param>
//        /// <returns>A task that represents the work queued to execute.</returns>
//        private InvokeResponse createAdaptiveCardInvokeResponseAsync(JObject authentication, string state, bool isBasicRefresh = false, string fileName = "AdaptiveCardResponse.json")
//        {
//            // Verify token is present or not.

//            bool isTokenPresent = authentication != null ? true : false;
//            bool isStatePresent = state != null && state != "" ? true : false;

//            string[] filepath = { ".", "Resources", fileName };

//            var adaptiveCardJson = File.ReadAllText(Path.Combine(filepath));
//            AdaptiveCard adaptiveCard = new AdaptiveCard();

//            var authResultData = isTokenPresent ? "SSO success" : isStatePresent ? "OAuth success" : "SSO/OAuth failed";

//            if (isBasicRefresh)
//            {
//                authResultData = "Refresh done";
//            }

//            var payloadData = new
//            {
//                authResult = authResultData,
//            };

//            var adaptiveCardResponse = new AdaptiveCardInvokeResponse()
//            {
//                StatusCode = 200,
//                Type = AdaptiveCard.ContentType,
//                Value = adaptiveCard
//            };

//            return CreateInvokeResponse(adaptiveCardResponse);
//        }

//        //    /// <summary>
//        //    /// when token is absent in the invoke. We can initiate SSO in response to the invoke
//        //    /// </summary>
//        //    /// <param name="turnContext">The context for the current turn.</param>
//        //    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
//        //    /// <returns>A task that represents the work queued to execute.</returns>
//        //    private async Task<InvokeResponse> initiateSSOAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
//        //    {
//        //        var signInLink = await GetSignInLinkAsync(turnContext, cancellationToken).ConfigureAwait(false);
//        //        var oAuthCard = new OAuthCard
//        //        {
//        //            Text = "Signin Text",
//        //            ConnectionName = _connectionName,
//        //            TokenExchangeResource = new TokenExchangeResource
//        //            {
//        //                Id = Guid.NewGuid().ToString()
//        //            },
//        //            Buttons = new List<CardAction>
//        //            {
//        //                new CardAction
//        //                {
//        //                    Type = ActionTypes.Signin,
//        //                    Value = signInLink,
//        //                    Title = "Please sign in",
//        //                },
//        //            }
//        //        };

//        //        var loginReqResponse = JObject.FromObject(new
//        //        {
//        //            statusCode = 401,
//        //            type = "application/vnd.microsoft.activity.loginRequest",
//        //            value = oAuthCard
//        //        });

//        //        return CreateInvokeResponse(loginReqResponse);
//        //    }
    }
}