// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Crazor.Server
{
    public partial class CardActivityHandler
    {
        protected async override Task<AdaptiveCardInvokeResponse> OnAdaptiveCardInvokeAsync(ITurnContext<IInvokeActivity> turnContext, AdaptiveCardInvokeValue invokeValue, CancellationToken cancellationToken)
        {
            AdaptiveAuthentication adaptiveAuthentication = null;
            try
            {
                CardRoute cardRoute = await CardRoute.FromDataAsync((JObject)invokeValue.Action.Data, Context.EncryptionProvider, cancellationToken);

                var cardApp = Context.CardAppFactory.Create(cardRoute, turnContext);

                await cardApp.LoadAppAsync((Activity)turnContext.Activity!, cancellationToken);

                AdaptiveCard card = await cardApp.ProcessInvokeActivity((Activity)turnContext.Activity!, isPreview: false, cancellationToken);

                return new AdaptiveCardInvokeResponse()
                {
                    StatusCode = 200,
                    Type = AdaptiveCard.ContentType,
                    Value = card
                };
            }
            catch (Microsoft.Identity.Web.MicrosoftIdentityWebChallengeUserException)
            {
                return CreateAuthCard(adaptiveAuthentication);
            }
            catch (UnauthorizedAccessException)
            {
                return CreateAuthCard(adaptiveAuthentication);
            }
        }

        private static AdaptiveCardInvokeResponse CreateAuthCard(AdaptiveAuthentication adaptiveAuthentication)
        {
            return new AdaptiveCardInvokeResponse()
            {
                StatusCode = 200,
                Type = AdaptiveCard.ContentType,
                Value = new AdaptiveCard("1.4")
                {
                    Body = new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock()
                            {
                                Text = "Unauthorized",
                                Wrap = true,
                            }
                        },
                    Actions = new List<AdaptiveAction>()
                        {
                            new AdaptiveSubmitAction()
                            {
                                Title = "Sign In",
                                Data = new AdaptiveCardInvokeAction()
                                {
                                    Verb = Constants.SHOWVIEW_VERB,
                                }
                            }
                        },
                    Authentication = adaptiveAuthentication
                }
            };
        }
    }
}