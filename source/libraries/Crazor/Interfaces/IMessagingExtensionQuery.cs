using Microsoft.Bot.Schema.Teams;

namespace Crazor.Interfaces
{
    public interface IMessagingExtensionQuery
    {
        Task<MessagingExtensionResponse> OnMessagingExtensionQueryAsync(MessagingExtensionQuery query, CancellationToken cancellationToken);
    }
}