// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using Crazor.Blazor.Components.AdaptiveCards;
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

        /// <summary>
        /// The CardRoute for this viewer.
        /// </summary>
        [Parameter]
        public string CardRoute { get; set; }

        /// <summary>
        /// Event which fires when card route changes.
        /// </summary>
        [Parameter]
        public EventCallback<string> OnCardRouteChanged { get; set; }

        protected override async Task OnInitializedAsync()
        {
            this._dotNetObjectRef = DotNetObjectReference.Create(this);
            this._botUrl = _configuration.GetValue<string>("BotUri") ?? new Uri(_configuration.GetValue<Uri>("HostUri"), "/api/cardapps").AbsoluteUri;
            this._channelId = _configuration.GetValue<Uri>("HostUri").Host;
            this._cardRoute = Crazor.CardRoute.Parse(CardRoute);
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
            this.CardRoute = this._cardApp.GetCurrentCardRoute();
            await OnCardRouteChanged.InvokeAsync(this.CardRoute);

            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (_jsModule == null)
            {
                _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", JAVASCRIPT_FILE);
            }
            string json = null;
            try
            {
                json = JsonConvert.SerializeObject(this._card);
            }
            catch (Exception err)
            {
                json = JsonConvert.SerializeObject(new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
                {
                    Body = new List<AdaptiveElement>()
                    {
                        new AdaptiveContainer()
                        {
                            Style = AdaptiveContainerStyle.Attention,
                            Items = new List<AdaptiveElement>() { new AdaptiveTextBlock() { Text = err.ToString(), Wrap = true, FontType = AdaptiveFontType.Monospace } }
                        }
                    }
                });
            }

            // Call javascript renderCrazorCard passing the cardId (aka the div) a reference to call back and the card to render
            await _jsModule.InvokeVoidAsync("renderCrazorCard", CardId, this._dotNetObjectRef, json);
            await base.OnAfterRenderAsync(firstRender);
        }


        /// <summary>
        /// Callback from javascript to handle processing an action.execute
        /// </summary>
        /// <param name="jsAction">the payload from the action.execute from js</param>
        /// <returns>async task</returns>
        [JSInvokable]
        public async Task onExecuteAction(object jsAction)
        {
            // turn js action into C# action.
            var action = JsonConvert.DeserializeObject<AdaptiveCardInvokeAction>(jsAction.ToString()!);

            // wrap it in an invoke activity
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

            // process it, giving us a new card
            this._card = await this._cardApp.ProcessInvokeActivity(activity, isPreview: false, default);
            this.CardRoute = this._cardApp.GetCurrentCardRoute();
            await OnCardRouteChanged.InvokeAsync(this.CardRoute);

            // tell tree to rerender. onrerender the card will be injected back into the html
            StateHasChanged();
        }
        public void Dispose() => _dotNetObjectRef?.Dispose();

    }
}
