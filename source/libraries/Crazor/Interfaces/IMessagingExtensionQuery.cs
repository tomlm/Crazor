// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;

namespace Crazor.Interfaces
{
    public interface IMessagingExtensionQuery
    {
        Task<MessagingExtensionResponse> OnMessagingExtensionQueryAsync(MessagingExtensionQuery query, CancellationToken cancellationToken);
    }
}