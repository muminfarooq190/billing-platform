namespace CommunicationService.Domain.ValueObjects;

public sealed record DeliveryAttempt(DateTimeOffset AttemptedAt, bool Success, string? ErrorMessage, string? ProviderMessageId);
