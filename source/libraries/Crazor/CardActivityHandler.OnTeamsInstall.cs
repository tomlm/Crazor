// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

//using Microsoft.Bot.Builder;
//using Microsoft.Bot.Builder.Teams;
//using Microsoft.Bot.Schema;
//using Microsoft.Bot.Schema.Teams;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;

//namespace Crazor
//{
//    // install status of a bot in a chat
//    public enum TeamsInstallStatus { Installed, NotInstalled, Exception };

//    public interface ITeamsInstall
//    {
//        public Task<TeamsInstallStatus> GetInstallStatusAsync(ITurnContext<IInvokeActivity> turnContext,
//            ILogger<CardActivityHandler> logger, CancellationToken cancellationToken);
//    }

//    public class TeamsInstall: ITeamsInstall
//    {
//        public async Task<TeamsInstallStatus> GetInstallStatusAsync(ITurnContext<IInvokeActivity> turnContext,
//            ILogger<CardActivityHandler> logger, CancellationToken cancellationToken)
//        {
//            try
//            {
//                // Check if your app is installed by fetching member information.
//                //
//                // As TeamsInfo.GetMemberAsync is a static class / method with connector, it's not easy for unit test,
//                // Therefore we wrap TeamsInfo.GetMemberAsync inside ITeamsInstall.GetInstallStatusAsync so that we can use Mock<ITeamInstall>()
//                // to override the behavior of TeamsInfo.GetMemberAsync
//                await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
//            }
//            catch (ErrorResponseException ex)
//            {
//                logger.LogError(ex, $"Teams GetInstallStatus failed when fetching members");
//                return (ex.Body.Error.Code == "BotNotInConversationRoster") ? TeamsInstallStatus.NotInstalled : TeamsInstallStatus.Exception;
//            }
//            return TeamsInstallStatus.Installed;
//        }
//    }

//    public partial class CardActivityHandler
//    {
//        // need to find a better way to put the json strings
//        // option 1: put as assembly resource. however, this complicates the current resource storage (as it treats all yaml/json files as power cards)
//        private const string _jit_noAction = "{\r\n  \"$schema\": \"http://adaptivecards.io/schemas/adaptive-card.json\",\r\n  \"version\": \"1.0\",\r\n  \"type\": \"AdaptiveCard\",\r\n  \"body\": [\r\n    {\r\n      \"type\": \"TextBlock\",\r\n      \"text\": \"The Power Apps Cards (bot) has been installed in this conversation !!\",\r\n      \"isSubtle\": false,\r\n      \"wrap\": true\r\n    }\r\n  ],\r\n  \"actions\": [\r\n    {\r\n      \"type\": \"Action.Submit\",\r\n      \"title\": \"Close\"\r\n    }\r\n  ]\r\n}\r\n";
//        private const string _jit_install = "{\r\n  \"type\": \"AdaptiveCard\",\r\n  \"body\": [\r\n    {\r\n      \"type\": \"TextBlock\",\r\n      \"text\": \"Looks like you haven't installed the Power Apps Cards (bot) in this team/chat. Please click **Install** to add this app.\",\r\n      \"wrap\": true\r\n    }\r\n  ],\r\n  \"actions\": [\r\n    {\r\n      \"type\": \"Action.Submit\",\r\n      \"title\": \"Install\",\r\n      \"data\": {\r\n        \"msteams\": {\r\n          \"justInTimeInstall\": true\r\n        }\r\n      }\r\n    }\r\n  ],\r\n  \"version\": \"1.0\"\r\n}\r\n";
//        private const string _jit_exception = "{\r\n  \"$schema\": \"http://adaptivecards.io/schemas/adaptive-card.json\",\r\n  \"version\": \"1.0\",\r\n  \"type\": \"AdaptiveCard\",\r\n  \"body\": [\r\n    {\r\n      \"type\": \"TextBlock\",\r\n      \"text\": \"There is an exception when trying to check if the Power Apps Cards (bot) has been installed or not for you in this chat.\",\r\n      \"isSubtle\": false,\r\n      \"wrap\": true\r\n    }\r\n  ],\r\n  \"actions\": [\r\n    {\r\n      \"type\": \"Action.Submit\",\r\n      \"title\": \"Close\"\r\n    }\r\n  ]\r\n}\r\n";

//        private ITeamsInstall _teamsInstall = new TeamsInstall();

//        internal static MessagingExtensionActionResponse GetMessagingExtensionActionResponse(string cardJson, string responseTitle)
//        {
//            var content = JsonConvert.DeserializeObject(cardJson);

//            return new MessagingExtensionActionResponse
//            {
//                Task = new TaskModuleContinueResponse
//                {
//                    Value = new TaskModuleTaskInfo
//                    {
//                        Card = new Attachment()
//                        {
//                            ContentType = "application/vnd.microsoft.card.adaptive",
//                            Content = content,
//                        },
//                        Height = 200,
//                        Width = 400,
//                        Title = responseTitle
//                    },
//                },
//            };
//        }

//        internal static async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync_JIT(
//            ITurnContext<IInvokeActivity> turnContext, ILogger<CardActivityHandler> logger,
//            ITeamsInstall teamsInstall, CancellationToken cancellationToken)
//        {
//            logger.LogInformation("Checking Install Status of Teams App");
//            var status = await teamsInstall.GetInstallStatusAsync(turnContext, logger, cancellationToken);
//            logger.LogInformation("Install Status of Teams App is {}", status);
//            switch(status)
//            {
//                case TeamsInstallStatus.Installed:
//                    return GetMessagingExtensionActionResponse(
//                        _jit_noAction,
//                        $"Welcome"
//                    );
//                case TeamsInstallStatus.NotInstalled:
//                    return GetMessagingExtensionActionResponse(
//                        _jit_install,
//                        "App Installation"
//                    );
//                default:  // e.g. TeamsInstallStatus.Exception
//                    return GetMessagingExtensionActionResponse(
//                        _jit_exception,
//                        "App Installation exception"
//                    );
//            }
//        }
//    }
//}