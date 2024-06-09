using Crazor;
using Crazor.Attributes;
using Crazor.Rendering;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace SharedCards.Cards.UnitTest
{
    public class UnitTestApp : CardApp
    {
        public UnitTestApp(CardAppContext context) : base(context)
        {
        }

        [SessionMemory]
        public ClientDetails Client { get; set; } = new ClientDetails();

        [SessionMemory]
        public Activity LastActivity { get; set; }

        public async override Task LoadAppAsync(IInvokeActivity activity, CancellationToken cancellationToken)
        {
            await base.LoadAppAsync(activity, cancellationToken);

            if (Context.TurnContext != null)
            {
                if (Context.TurnContext.Activity.Type == ActivityTypes.Message)
                {
                    Client.Activation = $"Message (Attachment)";
                    System.Diagnostics.Debug.WriteLine($"ACTIVATION: {Client.Activation}");
                }
                else if (Context.TurnContext.Activity.Type == ActivityTypes.Invoke &&
                    (Context.TurnContext.Activity.Name == "composeExtension/queryLink" ||
                     Context.TurnContext.Activity.Name == "composeExtension/anonymousQueryLink"))
                {
                    Client.Activation = $"Link Unfurled Card";
                    System.Diagnostics.Debug.WriteLine($"ACTIVATION: {Client.Activation}");
                }

                if (Client.ConversationType == ConversationType.Unknown)
                {
                    if (Context.TurnContext.Activity.ChannelId == Channels.Msteams && Enum.TryParse<ConversationType>(Context.TurnContext.Activity.Conversation.ConversationType, true, out var ct))
                    {
                        Client.ConversationType = ct;
                    }
                    else if (Context.TurnContext.Activity.ChannelId == Channels.Outlook || Context.TurnContext.Activity.ChannelId == Channels.Email)
                    {
                        Client.ConversationType = ConversationType.Email;
                    }
                }
            }
        }

        public async override Task SaveAppAsync(CancellationToken cancellationToken)
        {
            LastActivity = Context.TurnContext.Activity;
            await base.SaveAppAsync(cancellationToken);
        }


        public override Task OnActionExecuteAsync(CancellationToken cancellationToken)
        {
            if (ObjectPath.TryGetPathValue<string>(Action.Data, "Client.Activation", out var val))
            {
                Client.Activation = val;
            }

            return base.OnActionExecuteAsync(cancellationToken);

        }

        public async override Task<AdaptiveCard> RenderCardAsync(bool isPreview, CancellationToken cancellationToken)
        {
            var card = await base.RenderCardAsync(isPreview, cancellationToken);

            if (Client.Activation != null)
            {
                var visitor = new AdaptiveVisitor();
                visitor.Visit(card);
                foreach (var action in visitor.Elements.OfType<AdaptiveExecuteAction>())
                {
                    ObjectPath.SetPathValue(action.Data, "Client.Activation", Client.Activation);
                }

                foreach (var action in visitor.Elements.OfType<AdaptiveSubmitAction>())
                {
                    ObjectPath.SetPathValue(action.Data, "Client.Activation", Client.Activation);
                }
            }

            return card;
        }
    }
}
