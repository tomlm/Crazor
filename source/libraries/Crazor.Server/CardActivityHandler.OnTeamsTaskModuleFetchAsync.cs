using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
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
        protected async override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine($"Starting OnTeamsTaskModuleFetchAsync() processing");
            dynamic data = JObject.FromObject(taskModuleRequest.Data);
            string commandId = data.commandId;
            var uri = new Uri(Context.Configuration.GetValue<Uri>("HostUri")!, commandId);

            CardRoute cardRoute = CardRoute.FromUri(uri);

            var cardApp = Context.CardAppFactory.Create(cardRoute, turnContext);

            cardApp.IsTaskModule = true;
            var showActivity = turnContext.Activity.CreateLoadRouteActivity(commandId);
            await cardApp.LoadAppAsync(showActivity, cancellationToken);

            var card = await cardApp.ProcessInvokeActivity(turnContext.Activity, false, cancellationToken);

            switch (cardApp.TaskModuleAction)
            {
                case TaskModuleAction.Continue:

                    card.Refresh = null!;
                    var submitCard = TransformActionExecuteToSubmit(card);

                    // continue taskModule bound to current card view.
                    return new TaskModuleResponse()
                    {
                        Task = new TaskModuleContinueResponse()
                        {
                            Value = GetTaskInfoForCard(cardApp, submitCard)
                        },
                    };

                default:
                    await cardApp.SaveAppAsync(cancellationToken);

                    return new TaskModuleResponse() { };
            }
        }
    }
}