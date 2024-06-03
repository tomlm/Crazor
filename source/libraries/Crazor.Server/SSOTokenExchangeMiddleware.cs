using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;


namespace Crazor.Server
{
    /// <summary>
    /// If the activity name is signin/tokenExchange, this middleware will attempt to
    /// exchange the token, and deduplicate the incoming call, ensuring only one
    /// exchange request is processed.
    /// </summary>
    /// <remarks>
    /// If a user is signed into multiple Teams clients, the Bot could receive a
    /// "signin/tokenExchange" from each client. Each token exchange request for a
    /// specific user login will have an identical Activity.Value.Id.
    /// 
    /// Only one of these token exchange requests should be processed by the bot.
    /// The others return <see cref="System.Net.HttpStatusCode.PreconditionFailed"/>.
    /// For a distributed bot in production, this requires a distributed storage
    /// ensuring only one token exchange is processed. This middleware supports
    /// CosmosDb storage found in Microsoft.Bot.Builder.Azure, or MemoryStorage for
    /// local development. IStorage's ETag implementation for token exchange activity
    /// deduplication.
    /// </remarks>
    public class SSOTokenExchangeMiddleware : IMiddleware
    {
        private readonly IStorage _storage;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsSSOTokenExchangeMiddleware"/> class.
        /// </summary>
        /// <param name="storage">The <see cref="IStorage"/> to use for deduplication.</param>
        /// <param name="connectionName">The connection name to use for the single
        /// sign on token exchange.</param>
        public SSOTokenExchangeMiddleware(IStorage storage, IConfiguration configuration)
        {
            if (storage == null)
            {
                throw new ArgumentNullException(nameof(storage));
            }

            _storage = storage;
            _configuration = configuration;
        }

        /// <inheritdoc/>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {

            // if string.Equals(Channels.Msteams, turnContext.Activity.ChannelId, StringComparison.OrdinalIgnoreCase)
            if (turnContext.Activity.Type == ActivityTypes.Invoke || turnContext.Activity.Type == ActivityTypes.Event)
            {
                TokenExchangeInvokeRequest tokenExchangeRequest = null;

                if (string.Equals(turnContext.Activity.Name, SignInConstants.TokenExchangeOperationName, StringComparison.OrdinalIgnoreCase))
                {
                    ObjectPath.TryGetPathValue<TokenExchangeInvokeRequest>(turnContext.Activity, "value", out tokenExchangeRequest);
                }
                else if (string.Equals(turnContext.Activity.Name, "adaptiveCard/action", StringComparison.OrdinalIgnoreCase))
                {
                    ObjectPath.TryGetPathValue<TokenExchangeInvokeRequest>(turnContext.Activity, "value.authentication", out tokenExchangeRequest);
                }

                if (tokenExchangeRequest != null)
                {
                    // If the TokenExchange is NOT successful, the response will have already been sent by ExchangedTokenAsync
                    if (!await this.ExchangeTokenAsync(turnContext, tokenExchangeRequest, cancellationToken).ConfigureAwait(false))
                    {
                        return;
                    }

                    // Only one token exchange should proceed from here. Deduplication is performed second because in the case
                    // of failure due to consent required, every caller needs to receive the 
                    if (!await DeduplicatedTokenExchangeIdAsync(turnContext, cancellationToken).ConfigureAwait(false))
                    {
                        // If the token is not exchangeable, do not process this activity further.
                        return;
                    }
                }
            }

            await next(cancellationToken).ConfigureAwait(false);
        }

        private async Task<bool> DeduplicatedTokenExchangeIdAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // Create a StoreItem with Etag of the unique 'signin/tokenExchange' request
            var storeItem = new TokenStoreItem
            {
                ETag = (turnContext.Activity.Value as JObject).Value<string>("id")
            };

            var storeItems = new Dictionary<string, object> { { TokenStoreItem.GetStorageKey(turnContext), storeItem } };
            try
            {
                // Writing the IStoreItem with ETag of unique id will succeed only once
                await _storage.WriteAsync(storeItems, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)

                // Memory storage throws a generic exception with a Message of 'Etag conflict. [other error info]'
                // CosmosDbPartitionedStorage throws: ex.Message.Contains("pre-condition is not met")
                when (ex.Message.StartsWith("Etag conflict", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("pre-condition is not met"))
            {
                // Do NOT proceed processing this message, some other thread or machine already has processed it.

                // Send 200 invoke response.
                await SendInvokeResponseAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
                return false;
            }

            return true;
        }

        private async Task SendInvokeResponseAsync(ITurnContext turnContext, object body = null, HttpStatusCode httpStatusCode = HttpStatusCode.OK, CancellationToken cancellationToken = default)
        {
            await turnContext.SendActivityAsync(
                new Activity
                {
                    Type = ActivityTypesEx.InvokeResponse,
                    Value = new InvokeResponse
                    {
                        Status = (int)httpStatusCode,
                        Body = body,
                    },
                }, cancellationToken).ConfigureAwait(false);
        }

        private async Task<bool> ExchangeTokenAsync(ITurnContext turnContext, TokenExchangeInvokeRequest tokenExchangeInvokeRequest, CancellationToken cancellationToken)
        {
            var botId = _configuration.GetValue<string>("MicrosoftAppId");
            var hostUri = _configuration.GetValue<string>("HostUri");
            TokenResponse tokenExchangeResponse = null;
            string message = "The bot is unable to exchange token. Proceed with regular login.";
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            try
            {
                var tokenExchangeRequest = new TokenExchangeRequest
                {
                    Token = tokenExchangeInvokeRequest.Token,
                    // Uri = $"api://{new Uri(hostUri).Host}/BotId-{botId}"
                };

                if (userTokenClient != null)
                {
                    tokenExchangeResponse = await userTokenClient.ExchangeTokenAsync(
                        turnContext.Activity.From.Id,
                        tokenExchangeInvokeRequest.ConnectionName,
                        turnContext.Activity.ChannelId,
                        tokenExchangeRequest,
                        cancellationToken);
                }
                else if (turnContext.Adapter is IExtendedUserTokenProvider adapter)
                {
                    tokenExchangeResponse = await adapter.ExchangeTokenAsync(
                        turnContext,
                        tokenExchangeInvokeRequest.ConnectionName,
                        turnContext.Activity.From.Id,
                        tokenExchangeRequest,
                        cancellationToken);
                }
                else
                {
                    throw new NotSupportedException("Token Exchange is not supported by the current adapter.");
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types (ignoring, see comment below)
            catch (Exception err)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                message = err.Message;
                // Ignore Exceptions
                // If token exchange failed for any reason, tokenExchangeResponse above stays null,
                // and hence we send back a failure invoke response to the caller.
            }

            if (string.IsNullOrEmpty(tokenExchangeResponse?.Token))
            {
                // The token could not be exchanged (which could be due to a consent requirement)
                // Notify the sender that PreconditionFailed so they can respond accordingly.

                // https://learn.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/universal-actions-for-adaptive-cards/authentication-flow-in-universal-action-for-adaptive-cards
                //var response = JObject.FromObject(new
                //{
                //    statusCode = (int)HttpStatusCode.Unauthorized,
                //    type = "application/vnd.microsoft.error.invalidAuthCode"
                //});

                var response = JObject.FromObject(new
                {
                    statusCode = (int)HttpStatusCode.PreconditionFailed,
                    type = "application/vnd.microsoft.error.preconditionFailed",
                    value = new
                    {
                        code = "412",
                        message = message
                    }
                });

                await SendInvokeResponseAsync(turnContext, response, HttpStatusCode.OK, cancellationToken).ConfigureAwait(false);

                return false;
            }

            return true;
        }

        private class TokenStoreItem : IStoreItem
        {
            public string ETag { get; set; }

            public static string GetStorageKey(ITurnContext turnContext)
            {
                var activity = turnContext.Activity;
                var channelId = activity.ChannelId ?? throw new InvalidOperationException("invalid activity-missing channelId");
                var conversationId = activity.Conversation?.Id ?? throw new InvalidOperationException("invalid activity-missing Conversation.Id");

                var value = activity.Value as JObject;
                if (value == null || !value.ContainsKey("id"))
                {
                    throw new InvalidOperationException("Invalid signin/tokenExchange. Missing activity.Value.Id.");
                }

                return $"{channelId}/{conversationId}/{value.Value<string>("id")}";
            }
        }
    }
}
