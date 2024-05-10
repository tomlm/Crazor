


using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;

namespace Crazor.Server
{
    public partial class CardActivityHandler
    {
        /// <summary>
        /// Handle Fetch Task request
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async override Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine($"Starting OnTeamsTaskModuleSubmitAsync() processing");

            JObject data = JObject.FromObject(taskModuleRequest.Data);
            if (!data.ContainsKey(Constants.ROUTE_KEY) && data.ContainsKey("commandId"))
                data[Constants.ROUTE_KEY] = (string)data["commandId"];
            CardRoute cardRoute = await CardRoute.FromDataAsync(data, Context.EncryptionProvider, cancellationToken);

            AdaptiveCardInvokeValue invokeValue = Utils.TransfromSubmitDataToExecuteAction(data);

            var cardApp = Context.CardAppFactory.Create(cardRoute, turnContext);
            cardApp.IsTaskModule = true;

            var submitActionActivity = turnContext.Activity.CreateActionInvokeActivity(invokeValue);
            await cardApp.LoadAppAsync(submitActionActivity, cancellationToken);

            await cardApp.OnActionExecuteAsync(cancellationToken);

            switch (cardApp.TaskModuleAction)
            {
                case TaskModuleAction.Continue:
                    {
                        var adaptiveCard = await cardApp.RenderCardAsync(isPreview: false, cancellationToken);

                        await cardApp.SaveAppAsync(cancellationToken);

                        adaptiveCard.Refresh = null;
                        var submitCard = TransformActionExecuteToSubmit(adaptiveCard);
                        // continue taskModule bound to current card view.
                        return new TaskModuleResponse()
                        {
                            Task = new TaskModuleContinueResponse()
                            {
                                Value = GetTaskInfoForCard(cardApp, submitCard)
                            },
                        };
                    }

                case TaskModuleAction.Auto:
                case TaskModuleAction.PostCard:
                    {
                        var card = await cardApp.RenderCardAsync(isPreview: true, cancellationToken);

                        await cardApp.SaveAppAsync(cancellationToken);

                        var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
                        var reply = turnContext.Activity.CreateReply();
                        cardApp.IsTaskModule = false;
                        reply.Attachments.Add(new Attachment()
                        {
                            ContentType = AdaptiveCard.ContentType,
                            Content = card
                        });
                        reply.ChannelData = new
                        {
                            OnBehalfOf = new[]
                            {
                            new
                            {
                                ItemId = 0,
                                MentionType = "person",
                                Mri = turnContext.Activity.From.Id,
                                DisplayName = turnContext.Activity.From.Name
                            }
                        }
                        };

                        await connectorClient.Conversations.SendToConversationAsync(reply, cancellationToken);
                    }
                    return null!;
                case TaskModuleAction.InsertCard:
                case TaskModuleAction.None:
                default:
                    await cardApp.SaveAppAsync(cancellationToken);

                    return null!;
            }
        }

    }
}