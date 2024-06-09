using Crazor.Server;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
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

        private string _botName;

        private string _botId;

        private string _conversationId = Guid.NewGuid().ToString("n");

        private DotNetObjectReference<CardViewer> _dotNetObjectRef;

        //private CardApp _cardApp;

        //private CardRoute _cardRoute;

        private string _cardRoute;

        private AdaptiveCard _card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 5));

        [Inject]
        private IJSRuntime _jsRuntime { get; set; } = default!;

        private IJSObjectReference _jsModule { get; set; } = default!;

        [Inject]
        private IConfiguration _configuration { get; set; }

        [Inject]
        private IBot _bot { get; set; }

        [Inject]
        private CrazorCloudAdapter _adapter { get; set; }

        [Inject]
        private MicrosoftIdentityConsentAndConditionalAccessHandler ConsentHandler { get; set; }


        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationState { get; set; }

        public CardViewer()
        {
        }

        public string CardId { get; set; } = $"card{Utils.GetNewId()}";

        [Parameter]
        public string Route
        {
            get => _cardRoute;
            set
            {
                if (String.Compare(_cardRoute, value, true) != 0)
                {
                    _cardRoute = CardRoute.Parse(value).Route;
                }
            }
        }


        /// <summary>
        /// Event which fires when card route changes.
        /// </summary>
        [Parameter]
        public EventCallback<string> OnCardRouteChanged { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                this._dotNetObjectRef = DotNetObjectReference.Create(this);
                this._botUrl = _configuration.GetValue<string>("BotUri") ?? new Uri(_configuration.GetValue<Uri>("HostUri") ?? throw new ArgumentNullException("HostUri"), "/api/cardapps").AbsoluteUri;
                this._channelId = _configuration.GetValue<Uri>("HostUri")!.Host;
                this._botName = _configuration.GetValue<string>("BotName")!;
                this._botId = _configuration.GetValue<string>("MicrosoftAppId")!;

                await LoadRouteAsync(Route);

                await base.OnInitializedAsync();
            }
            catch (MicrosoftIdentityWebChallengeUserException err)
            {
                ConsentHandler.HandleException(err);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (_jsModule == null)
            {
                _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", JAVASCRIPT_FILE);
            }
            string? json = null;
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
            var action = JsonConvert.DeserializeObject<AdaptiveCardInvokeAction>(jsAction.ToString()!)!;
            var authState = (AuthenticationState != null) ? await AuthenticationState : null;
            // wrap it in an invoke activity
            var activity = new Activity(ActivityTypes.Invoke)
            {
                ServiceUrl = _botUrl,
                ChannelId = _channelId,
                Id = Guid.NewGuid().ToString("n"),
                From = new ChannelAccount()
                {
                    AadObjectId = authState?.User?.GetObjectId() ?? String.Empty,
                    Id = authState?.User?.GetObjectId() ?? "Unknown",
                    Name = authState?.User?.GetDisplayName() ?? "Anonymous"
                },
                Recipient = new ChannelAccount() { Id = _botId, Name = _botName },
                Conversation = new ConversationAccount() { Id = _conversationId },
                Timestamp = DateTimeOffset.UtcNow,
                LocalTimestamp = DateTimeOffset.Now,
            }
            .CreateActionInvokeActivity(action.Verb, JObject.FromObject(action.Data));

            // process it, giving us a new card
            var invokeResponse = await _adapter.ProcessAuthenticatedActivityAsync((Activity)activity!, (tc, ct) => _bot.OnTurnAsync(tc, ct), default);
            await ProcessInvokeResponse(invokeResponse);
        }

        public async Task LoadRouteAsync(string route)
        {
            var authState = (AuthenticationState != null) ? await AuthenticationState : null;
            // wrap it in an invoke activity
            var activity = new Activity(ActivityTypes.Invoke)
            {
                ServiceUrl = _botUrl,
                ChannelId = _channelId,
                Id = Guid.NewGuid().ToString("n"),
                From = new ChannelAccount()
                {
                    AadObjectId = authState?.User?.GetObjectId() ?? String.Empty,
                    Id = authState?.User?.GetObjectId() ?? "Unknown",
                    Name = authState?.User?.GetDisplayName() ?? "Anonymous"
                },
                Recipient = new ChannelAccount() { Id = _botId, Name = _botName },
                Conversation = new ConversationAccount() { Id = _conversationId },
                Timestamp = DateTimeOffset.UtcNow,
                LocalTimestamp = DateTimeOffset.Now,
            }
            .CreateLoadRouteActivity(route);

            //if (authState != null)
            //{
            //    _cardApp.Context.User = authState.User;
            //}

            var invokeResponse = await _adapter.ProcessAuthenticatedActivityAsync((Activity)activity!, (tc, ct) => _bot.OnTurnAsync(tc, ct), default);
            await ProcessInvokeResponse(invokeResponse);
        }

        private async Task ProcessInvokeResponse(InvokeResponse invokeResponse)
        {
            var acResponse = invokeResponse.Body as AdaptiveCardInvokeResponse;
            if (acResponse != null)
            {
                if (acResponse.StatusCode >= 200 && acResponse.StatusCode < 300)
                {
                    if (acResponse.Value is AdaptiveCard card)
                    {
                        _card = card;
                    }
                    else
                    {
                        _card = CreateErrorCard(JsonConvert.SerializeObject(invokeResponse));
                    }
                }
            }
            else
            {
                _card = CreateErrorCard(JsonConvert.SerializeObject(invokeResponse));
            }

            // notify host that route is now different
            if (_card.Metadata?.WebUrl != null)
            {
                this.Route = new Uri(_card.Metadata.WebUrl).PathAndQuery;
                await OnCardRouteChanged.InvokeAsync(this.Route);
            }

            // tell tree to rerender. onrerender the card will be injected back into the html
            StateHasChanged();
        }

        public void Dispose() => _dotNetObjectRef?.Dispose();

        public AdaptiveCard CreateErrorCard(string message)
        {
            return new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveTextBlock(this.Route) { Style = AdaptiveTextBlockStyle.Heading },
                    new AdaptiveTextBlock(message)
                }
            };
        }
    }
}
