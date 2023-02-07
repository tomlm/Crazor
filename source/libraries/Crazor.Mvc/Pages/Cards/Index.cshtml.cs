// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveCards;
using Crazor.Server.Controllers;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Crazor.Mvc.Pages.Cards
{
    public class CardHostModel : PageModel
    {
        private static HttpClient _httpClient = new HttpClient();
        private CardAppFactory _cardAppFactory;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CardHostModel(CardAppContext context)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            BotUri = context.Configuration.GetValue<string>("BotUri") ?? new Uri(context.Configuration.GetValue<Uri>("HostUri"), "/api/cardapps").AbsoluteUri;
            ChannelId = context.Configuration.GetValue<Uri>("HostUri").Host;
            Context = context;
            CardId = $"card{Utils.GetNewId()}";
        }

        public string CardId { get; set; }

        public string BotUri { get; set; }

        public string ChannelId { get; set; }

        public CardApp? CardApp { get; set; }

        public AdaptiveCard? AdaptiveCard { get; set; }

        public string RouteUrl { get; set; }

        public CardAppContext Context { get; }

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
