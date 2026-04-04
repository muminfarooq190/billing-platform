namespace CommunicationService.Api.Contracts;

public sealed record SendNotificationRequest(
    Guid TenantId,
    Guid RecipientId,
    string RecipientType,
    string? Channel,
    string? TemplateName,
    string? Subject,
    string? Body,
    string Priority,
    string? ReferenceId,
    Dictionary<string, string>? Placeholders);
