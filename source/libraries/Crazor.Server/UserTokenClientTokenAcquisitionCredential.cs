using Azure.Core;
using Crazor.Attributes;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using System.Reflection;
using System.Security.Claims;

namespace Crazor.Server
{
    internal class CrazorAuthorizationHeaderProvider : TokenCredential, IAuthorizationHeaderProvider
    {
        public CrazorAuthorizationHeaderProvider(CardAppContext context, ITokenAcquisition tokenAcquirer)
        {
            Context = context;
            TokenAcquisition = tokenAcquirer;
        }

        public CardAppContext Context { get; }

        public ITokenAcquisition TokenAcquisition { get; }

        public async Task<string> CreateAuthorizationHeaderForAppAsync(string scopes, AuthorizationHeaderProviderOptions? downstreamApiOptions = null, CancellationToken cancellationToken = default)
        {
            var result = await TokenAcquisition.GetAccessTokenForAppAsync(scopes);
            return result;
        }

        public async Task<string> CreateAuthorizationHeaderForUserAsync(IEnumerable<string> scopes, AuthorizationHeaderProviderOptions? authorizationHeaderProviderOptions = null, ClaimsPrincipal? claimsPrincipal = null, CancellationToken cancellationToken = default)
        {
            var authenticationAttribute = Context.App.CurrentView.GetType().GetCustomAttribute<OAuthConnectionAttribute>();
            if (authenticationAttribute != null)
            {
                var userTokenClient = Context.TurnContext.TurnState?.Get<UserTokenClient>();
                if (userTokenClient != null)
                {
                    var tokenResponse = await userTokenClient.GetUserTokenAsync(Context.App.Activity.From.Id, authenticationAttribute.Connection, Context.App.Activity.ChannelId, null, cancellationToken);

                    if (tokenResponse != null)
                    {
                        return tokenResponse.Token;
                    }
                    else
                    {
                        var result = await TokenAcquisition.GetAccessTokenForUserAsync(scopes);
                        return result;
                    }
                }
            }

            return null;
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return this.GetTokenAsync(requestContext, cancellationToken).GetAwaiter().GetResult();
        }

        public async override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var authenticationAttribute = Context.App.CurrentView.GetType().GetCustomAttribute<OAuthConnectionAttribute>();
            if (authenticationAttribute != null)
            {
                var userTokenClient = Context.TurnContext.TurnState?.Get<UserTokenClient>();
                if (userTokenClient != null)
                {
                    var tokenResponse = await userTokenClient.GetUserTokenAsync(Context.App.Activity.From.Id, authenticationAttribute.Connection, Context.App.Activity.ChannelId, null, cancellationToken);

                    if (tokenResponse != null)
                    {
                        return new AccessToken(tokenResponse.Token, DateTimeOffset.Parse(tokenResponse.Expiration));
                    }
                    else
                    {
                        var result = await TokenAcquisition.GetAccessTokenForUserAsync(requestContext.Scopes);
                        tokenResponse = new TokenResponse(Context.App.Activity.ChannelId, token: result);
                        return new AccessToken(tokenResponse.Token, DateTimeOffset.Parse(tokenResponse.Expiration));
                    }
                }
            }

            return default;
        }
    }
}
