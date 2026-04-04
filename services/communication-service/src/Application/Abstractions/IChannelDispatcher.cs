using CommunicationService.Domain.Enums;

namespace CommunicationService.Application.Abstractions;

public interface IChannelDispatcher
{
    ChannelType Channel { get; }
    Task<ChannelDispatchResult> SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken);
}

public sealed record ChannelDispatchResult(bool Success, string? ProviderMessageId, string? ErrorMessage);
