//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using AdaptiveCards;
using Crazor.Attributes;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Mime;
using System.Reflection;
using System.Security.Policy;

namespace Crazor
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
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            CancellationToken cancellationToken)
        {
            _logger!.LogInformation($"Starting OnTeamsMessagingExtensionSubmitActionAsync() ");

            var uri = new Uri(_configuration.GetValue<Uri>("HostUri"), action.CommandId);

            var cardApp = await LoadAppAsync(turnContext, uri, cancellationToken);
            cardApp.IsTaskModule = true;
            cardApp.MessageExtensionAction = action;

            // reconstruct actionexecute from .data[Action.Execute]
            var actionExecute = JObject.FromObject(action.Data)[AdaptiveExecuteAction.TypeName].ToObject<AdaptiveExecuteAction>()!;
            // gin up a AdaptiveCardInvokeAction
            cardApp.Action = new AdaptiveCardInvokeAction()
            {
                Data = actionExecute.Data,
                Id = actionExecute.Id,
                Verb = actionExecute.Verb,
            };

            // copy data over (skipping Action.Execute property)
            foreach (var property in ((JObject)action.Data).Properties().Where(property => property.Name != AdaptiveExecuteAction.TypeName))
            {
                ((JObject)cardApp.Action.Data)[property.Name] = property.Value;
            }

            var adaptiveCard = await cardApp.OnActionExecuteAsync(cancellationToken);
            await cardApp.SaveAppAsync(cancellationToken);

            switch (cardApp.TaskModuleStatus)
            {
                case TaskModuleStatus.Continue:
                    // continue taskModule bound to current card view.
                    return new MessagingExtensionActionResponse()
                    {
                        Task = new TaskModuleContinueResponse()
                        {
                            Value = GetTaskInfoForCard(cardApp, adaptiveCard)
                        },
                    };

                case TaskModuleStatus.Insert:
                    // insert card into output
                    return new MessagingExtensionActionResponse
                    {
                        ComposeExtension = new MessagingExtensionResult(attachmentLayout: "list", type: "result")
                        {
                            // url to card
                            Text = new Uri(_configuration.GetValue<Uri>("HostUri"), cardApp.GetRoute()).AbsoluteUri,
                            Attachments = new List<MessagingExtensionAttachment>()
                            {
                                // card
                                new MessagingExtensionAttachment(AdaptiveCard.ContentType, content: adaptiveCard)
                            }
                        },
                    };

                case TaskModuleStatus.Post:
                case TaskModuleStatus.None:
                default:
                    return new MessagingExtensionActionResponse() { };
            }
        }
    }
}