namespace CommunicationService.Api.Contracts;

public sealed record SendNotificationRequest(
    Guid RecipientId,
    string RecipientType,
    string? Channel,
    string? TemplateName,
    string? Subject,
    string? Body,
    string Priority,
    string? ReferenceId,
    string? CorrelationId,
    string? IdempotencyKey,
    List<DocumentReferenceRequest>? Documents,
    Dictionary<string, string>? Metadata,
    Dictionary<string, string>? Placeholders);

public sealed record SendWorkflowNotificationRequest(
    Guid RecipientId,
    string RecipientType,
    string? Channel,
    string? TemplateName,
    string? Subject,
    string? Body,
    string? Priority,
    string? ReferenceId,
    string? CorrelationId,
    string? IdempotencyKey,
    List<DocumentReferenceRequest>? Documents,
    Dictionary<string, string>? Metadata,
    Dictionary<string, string>? Placeholders);

public sealed record DocumentReferenceRequest(
    string Name,
    string? DocumentId,
    string? Url,
    string? ContentType,
    long? SizeBytes,
    Dictionary<string, string>? Metadata);

public sealed record ReplayNotificationRequest(string? Reason);
