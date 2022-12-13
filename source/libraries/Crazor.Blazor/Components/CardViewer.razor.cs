﻿using AdaptiveCards;
using Microsoft.AspNetCore.Components;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Crazor.Blazor.Components
{
    /// <summary>
    /// This class renders a Crazor card by route reference.
    /// </summary>
    public partial class CardViewer : IDisposable
    {
        private const string JAVASCRIPT_FILE = "./_content/Crazor.Blazor/Components/CardViewer.razor.js";

        private string _botUrl;

        private string _channelId;

        private DotNetObjectReference<CardViewer> _dotNetObjectRef;

        private CardApp _cardApp;

        private CardRoute _cardRoute;

        private AdaptiveCard _card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 5));

        [Inject]
        private IJSRuntime _jsRuntime { get; set; } = default!;

        private IJSObjectReference _jsModule { get; set; } = default!;

        [Inject]
        private IConfiguration _configuration { get; set; }

        [Inject]
        private CardAppFactory _cardAppFactory { get; set; }

        public CardViewer()
        {
        }

        public string CardId { get; set; } = $"card{Utils.GetNewId()}";

        [Parameter]
        public string Route { get; set; }

        protected override async Task OnInitializedAsync()
        {
            this._dotNetObjectRef = DotNetObjectReference.Create(this);
            this._botUrl = _configuration.GetValue<string>("BotUri") ?? new Uri(_configuration.GetValue<Uri>("HostUri"), "/api/cardapps").AbsoluteUri;
            this._channelId = _configuration.GetValue<Uri>("HostUri").Host;
            this._cardRoute = CardRoute.Parse(Route);
            this._cardApp = _cardAppFactory.Create(_cardRoute);

            var activity = new Activity(ActivityTypes.Invoke)
            {
                ServiceUrl = "https://about",
                ChannelId = this._channelId,
                Id = Guid.NewGuid().ToString("n"),
                From = new ChannelAccount() { Id = String.Empty },
                Recipient = new ChannelAccount() { Id = "bot" },
                Conversation = new ConversationAccount() { Id = Utils.GetNewId() },
                Timestamp = DateTimeOffset.UtcNow,
                LocalTimestamp = DateTimeOffset.Now,
            }
            .CreateLoadRouteActivity(_cardRoute.Route);

            this._card = await this._cardApp.ProcessInvokeActivity(activity, isPreview: false, default);
            this.Route = this._cardApp.GetCurrentCardRoute();

            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (_jsModule == null)
            {
                _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", JAVASCRIPT_FILE);
            }
            await _jsModule.InvokeVoidAsync("renderCrazorCard", CardId, this._dotNetObjectRef, JsonConvert.SerializeObject(this._card));
            await base.OnAfterRenderAsync(firstRender);
        }

        [JSInvokable]
        public async Task onExecuteAction(object actionIn)
        {
            var action = JsonConvert.DeserializeObject<AdaptiveCardInvokeAction>(actionIn.ToString()!);

            var activity = new Activity(ActivityTypes.Invoke)
            {
                ServiceUrl = "https://about",
                ChannelId = this._channelId,
                Id = Guid.NewGuid().ToString("n"),
                From = new ChannelAccount() { Id = String.Empty },
                Recipient = new ChannelAccount() { Id = "bot" },
                Conversation = new ConversationAccount() { Id = _cardRoute.SessionId },
                Timestamp = DateTimeOffset.UtcNow,
                LocalTimestamp = DateTimeOffset.Now,
            }
            .CreateActionInvokeActivity(action.Verb ?? Constants.SHOWVIEW_VERB, JObject.FromObject(action.Data));

            this._card = await this._cardApp.ProcessInvokeActivity(activity, isPreview: false, default);
            var json = JsonConvert.SerializeObject(_card);

            StateHasChanged();
        }
        public void Dispose() => _dotNetObjectRef?.Dispose();

    }
}
