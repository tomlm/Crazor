



//namespace Crazor.Server
//{
//    // this is the payload to be embedded in a card
//    // for user to click to trigger and open a task module
//    public class CardTaskFetchValueForUser<T>
//    {
//        [JsonProperty("msteams")]
//        public object Type { get; set; } = JsonConvert.DeserializeObject("{\"type\": \"task/fetch\" }");

//        [JsonProperty("data")]
//        public T Data { get; set; }
//    }

//    // this is the payload Teams send back to bot
//    // after user clicks the button on card for opening a task module
//    public class CardTaskFetchValueToBot<T>
//    {
//        [JsonProperty("type")]
//        public object Type { get; set; } = "task/fetch";

//        [JsonProperty("data")]
//        public T Data { get; set; }
//    }

//    // User consent flow using via web flow embedded in a task module
//    public partial class CardBot
//    {
//        // Get a demo adaptive card that contains a button to trigger a task module
//        public static AdaptiveCard GetDemoConsentCard()
//        {
//            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 4))
//            {
//                Body = new List<AdaptiveElement>()
//                    {
//                        new AdaptiveTextBlock(){ Text="Task Module Invocation from Card",
//                            Weight=AdaptiveTextWeight.Bolder,
//                            Size=AdaptiveTextSize.Large}
//                    },
//                Actions = new[] { "consent_flow" }
//                  .Select(actionId => new AdaptiveSubmitAction()
//                  {
//                      Title = actionId,
//                      Data = new CardTaskFetchValueForUser<string>() { Data = actionId }
//                  })
//                  .ToList<AdaptiveAction>(),
//            };
//            return card;
//        }

//        // This function sends a card to chat where its button is able to popup a task module
//        //
//        // It also returns a card telling about the demo.
//        // Ideally it is not necessary to show up,
//        // but we need to return something for message extension command.
//        internal static async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync_DemoCardForTaskModuleConsent(
//            ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
//        {
//            var card = GetDemoConsentCard();
//            var attachment = new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
//            var messageActivity = MessageFactory.Attachment(new[] { attachment });
//            await turnContext.SendActivityAsync(messageActivity, cancellationToken);

//            // describing the demo
//            AdaptiveCard responseCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 4));
//            var nl = System.Environment.NewLine;
//            responseCard.Body.Add(new AdaptiveTextBlock()
//            { 
//                Text =  @$"This demo shows consent flow in a task module.{nl}
//                           Please close this task module,{nl}
//                           then click consent_flow button.",
//            });
//            return GetMessagingExtensionActionResponse(responseCard.ToJson(), "Demo Description");
//        }

//    // a consent action button triggers this
//    protected override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(
//            ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
//        {
//            var asJobject = JObject.FromObject(taskModuleRequest.Data);
//            var value = asJobject.ToObject<CardTaskFetchValueToBot<string>>()?.Data;

//            var taskInfo = new TaskModuleTaskInfo();
//            switch (value)
//            {
//                case "consent_flow":
//                    // TODO: link to the correct place, also depending on ppux env
//                    taskInfo.Height = "medium";
//                    taskInfo.Width = "medium";
//                    taskInfo.Title = "Welcome to the card gallery";
//                    taskInfo.Url = GetUrl("ppux_test");
//                    break;
//                default:
//                    break;
//            }

//            var response = new TaskModuleResponse
//            {
//                Task = new TaskModuleContinueResponse() { Value = taskInfo }
//            };

//            return Task.FromResult(response);
//        }
//    }
//}