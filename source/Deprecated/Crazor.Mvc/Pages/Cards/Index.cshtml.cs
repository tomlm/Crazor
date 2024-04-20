// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.AdaptiveCards;
using Crazor.Server.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Reflection;

namespace Crazor.Mvc.Pages.Cards
{
    public class CardHostModel : PageModel
    {
        private static HttpClient _httpClient = new HttpClient();
        private CardAppFactory _cardAppFactory;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CardHostModel(CardAppContext context, IAuthorizationService authorizationService, AuthenticationStateProvider authenticationStateProvider)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            BotUri = context.Configuration.GetValue<string>("BotUri") ?? new Uri(context.Configuration.GetValue<Uri>("HostUri"), "/api/cardapps").AbsoluteUri;
            ChannelId = context.Configuration.GetValue<Uri>("HostUri").Host;
            Context = context;
            AuthorizationService = authorizationService;
            AuthenticationStateProvider = authenticationStateProvider;
            CardId = $"card{Utils.GetNewId()}";
        }

        public string CardId { get; set; }

        public string BotUri { get; set; }

        public string ChannelId { get; set; }

        public CardApp? CardApp { get; set; }

        public AdaptiveCard? AdaptiveCard { get; set; }

        public string RouteUrl { get; set; }

        public CardAppContext Context { get; }
        public IAuthorizationService AuthorizationService { get; }
        public AuthenticationStateProvider AuthenticationStateProvider { get; }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            string userId;
            if (this.Request.Cookies.TryGetValue("userId", out var uid))
            {
                userId = uid!;
            }
            else
            {
                userId = Utils.GetNewId();
            }

            var uri = new Uri(Request.GetDisplayUrl());
            var cardRoute = CardRoute.FromUri(uri);
            this.CardApp = Context.CardAppFactory.Create(cardRoute, null);

            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            this.CardApp.Context.User = authState.User;

            ArgumentNullException.ThrowIfNull(this.CardApp);
            var channelId = Request.Headers["x-channel-id"].FirstOrDefault() ?? this.ChannelId;

            var loadRouteActivity = new Activity(ActivityTypes.Invoke)
            {
                ServiceUrl = "https://about",
                ChannelId = channelId,
                Id = Guid.NewGuid().ToString("n"),
                From = new ChannelAccount() { Id = userId },
                Recipient = new ChannelAccount() { Id = "bot" },
                Conversation = new ConversationAccount() { Id = Utils.GetNewId() },
                Timestamp = DateTimeOffset.UtcNow,
                LocalTimestamp = DateTimeOffset.Now,
            }
            .CreateLoadRouteActivity(cardRoute.Route);

            var token = await CardAppController.GetTokenAsync(Context.Configuration);
            this.Response.Cookies.Append("token", token);
            this.Response.Cookies.Append("userId", userId);

            await CardApp.LoadAppAsync(loadRouteActivity!, default);

            // Hmmm...where is a better place to put this block of code?
            var authorizeAttributes = CardApp.CurrentView.GetType().GetCustomAttributes<AuthorizeAttribute>().ToList();
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

            this.AdaptiveCard = await CardApp.ProcessInvokeActivity(loadRouteActivity, isPreview: false, cancellationToken);

            this.RouteUrl = this.CardApp.GetCurrentCardRoute();


            var accept = Request.Headers.Accept.FirstOrDefault();
            if (accept != null)
            {
                var contentTypes = accept.Split(',');
                foreach (var contentType in contentTypes)
                {
                    switch (contentType)
                    {
                        case AdaptiveCard.ContentType:
                            Response.ContentType = AdaptiveCard.ContentType;
                            return Content(JsonConvert.SerializeObject(AdaptiveCard));
                        case "application/json":
                            Response.ContentType = "application/json";
                            return Content(JsonConvert.SerializeObject(AdaptiveCard));
                        case "text/html":
                            return null!;
                    }
                }
            }
            return null!;
        }
    }
}
