// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveCards;
using Crazor.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;
using System.Security.Claims;

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

        /// <summary>
        /// Authorize the activity for the user.
        /// </summary>
        /// <remarks>
        /// * This will assign Context.User and Context.UserToken if the user is authorized.
        /// </remarks>
        /// <exception cref="UnauthorizedAccessException">It will throw an UnauthorizedAccessException if the authenticated user does not meet [Authorized] attribute definition.</exception>
        /// <param name="invokeActivity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>It will return Authentication if the user is not authenticated.</returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        private async Task<AdaptiveAuthentication> AuthorizeActivityAsync(CardApp cardApp, ITurnContext<IInvokeActivity> turnContext, bool isPreview, CancellationToken cancellationToken)
        {
            var userTokenClient = turnContext?.TurnState.Get<UserTokenClient>();

            // Authenticate user identity by using token exchange
            var authenticationAttribute = cardApp.CurrentView.GetType().GetCustomAttribute<AuthenticationAttribute>();
            if (authenticationAttribute != null)
            {
                if (userTokenClient == null)
                {
                    throw new UnauthorizedAccessException($"No TurnContext");
                }

                ObjectPath.TryGetPathValue<string>(turnContext.Activity.Value, "state", out var magicCode);

                Context.UserToken = await userTokenClient.GetUserTokenAsync(turnContext.Activity.From.Id, authenticationAttribute.Name, turnContext.Activity.ChannelId, magicCode, cancellationToken);

                if (Context.UserToken != null)
                {
                    // parse token into identity and claims.
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jwtSecurityToken = tokenHandler.ReadJwtToken(Context.UserToken.Token);
                    var claimsIdentity = new ClaimsIdentity(jwtSecurityToken.Claims, "JWT");
                    Context.User = new ClaimsPrincipal(claimsIdentity);
                }
            }

            // make sure that user is set into the authenticationStateProvider is initialized
            var isp = this.AuthenticationStateProvider as IHostEnvironmentAuthenticationStateProvider;
            if (isp != null)
            {
                isp.SetAuthenticationState(Task.FromResult(new AuthenticationState(Context.User)));
            }

            // Authorize user by processing authorize attributes
            var authorizeAttributes = cardApp.CurrentView.GetType().GetCustomAttributes<AuthorizeAttribute>().ToList();
            if (authorizeAttributes.Any())
            {
                // if we are not authenticated and there are Authorize attributes then we just blow out of here.
                if (Context.User?.Identity.IsAuthenticated == false)
                {
                    throw new UnauthorizedAccessException();
                }

                foreach (var authorizeAttribute in authorizeAttributes)
                {
                    // if we have a policy then validate it.
                    if (!String.IsNullOrEmpty(authorizeAttribute.Policy))
                    {
                        var result = await AuthorizationService.AuthorizeAsync(Context.User!, authorizeAttribute.Policy);
                        if (result.Failure != null)
                        {
                            throw new UnauthorizedAccessException(String.Join("\n", result.Failure.FailureReasons.Select(reason => reason.Message)));
                        }
                    }

                    // if we have roles, then validate them.
                    if (!String.IsNullOrEmpty(authorizeAttribute.Roles))
                    {
                        foreach (var role in authorizeAttribute.Roles.Split(',').Select(r => r.Trim()))
                        {
                            if (!Context.User.IsInRole(role))
                            {
                                throw new UnauthorizedAccessException($"User is not in required role [{role}]");
                            }
                        }
                    }
                }
            }

            // If we are not authenticated, then return authentication metadata.
            if (authenticationAttribute != null)
            {
                var appId = Context.Configuration.GetValue<string>("MicrosoftAppId");

                var signinResource = await userTokenClient.GetSignInResourceAsync(authenticationAttribute.Name, (Activity)turnContext.Activity, null, cancellationToken);

                // create authenticationOptions to ask to be logged in.
                return new AdaptiveAuthentication()
                {
                    ConnectionName = authenticationAttribute.Name,
                    Text = "Please sign in to continue",
                    Buttons = new List<AdaptiveAuthCardButton>
                    {
                        new AdaptiveAuthCardButton()
                        {
                            Title = "Sign In",
                            Type = "signin",
                            Value = signinResource.SignInLink
                        }
                    },
                    TokenExchangeResource = JObject.FromObject(signinResource.TokenExchangeResource).ToObject<AdaptiveTokenExchangeResource>()!,
                };
            }

            // we are good, we are authenticated and authorized.
            return null;
        }

    }
}