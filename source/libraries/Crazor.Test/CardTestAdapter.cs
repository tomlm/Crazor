// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Newtonsoft.Json;
using System.Globalization;
using System.Net;

namespace Crazor.Test
{

    public class CardTestAdapter : TestAdapter
    {
        private readonly object _lock = new object();
        private int _nextId = 0;
        private IBot _bot;

        public CardTestAdapter(IBot bot)
        {
            _bot = bot;
        }

        public Uri BaseUri { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public JsonSerializerSettings SerializationSettings => throw new NotImplementedException();

        public JsonSerializerSettings DeserializationSettings => throw new NotImplementedException();

        public ServiceClientCredentials Credentials => throw new NotImplementedException();

        public IAttachments Attachments => throw new NotImplementedException();

        public IConversations Conversations => throw new NotImplementedException();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async virtual Task<InvokeResponse> Invoke(IInvokeActivity activity, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                // ready for next reply
                if (activity.Type == null)
                {
                    activity.Type = ActivityTypes.Message;
                }

                if (activity.ChannelId == null)
                {
                    activity.ChannelId = Conversation.ChannelId;
                }

                if (activity.From == null || activity.From.Id == "unknown" || activity.From.Role == RoleTypes.Bot)
                {
                    activity.From = Conversation.User;
                }

                activity.Recipient = Conversation.Bot;
                activity.Conversation = Conversation.Conversation;
                activity.ServiceUrl = Conversation.ServiceUrl;

                var id = activity.Id = (_nextId++).ToString(CultureInfo.InvariantCulture);
            }

            if (activity.Timestamp == null || activity.Timestamp == default(DateTimeOffset))
            {
                activity.Timestamp = DateTimeOffset.UtcNow;
            }

            if (activity.LocalTimestamp == null || activity.LocalTimestamp == default(DateTimeOffset))
            {
                activity.LocalTimestamp = DateTimeOffset.Now;
            }

            // note here Dispose is NOT called on the TurnContext because we want to use it later in the test code
            var context = CreateTurnContext((Activity)activity);

            await RunPipelineAsync(context, _bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);

            if (activity.Type == ActivityTypes.Invoke)
            {
                // Handle Invoke scenarios, which deviate from the request/request model in that
                // the Bot will return a specific body and return code.
                var response = this.ActiveQueue.SingleOrDefault(a => a.Type == ActivityTypesEx.InvokeResponse);
                if (response == null)
                {
                    return new InvokeResponse { Status = (int)HttpStatusCode.NotImplemented };
                }
                else
                {
                    activity = this.ActiveQueue.Single(a => a.Type == ActivityTypesEx.InvokeResponse);
                    var list = this.ActiveQueue.Where(a => a.Type != ActivityTypesEx.InvokeResponse).ToList();
                    this.ActiveQueue.Clear();
                    foreach (var item in list)
                        this.ActiveQueue.Enqueue(item);

                    return (InvokeResponse)activity.Value;
                }
            }

            return null;
        }
    }
}
