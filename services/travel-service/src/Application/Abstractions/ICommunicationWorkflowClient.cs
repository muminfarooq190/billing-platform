namespace TravelService.Application.Abstractions;

public interface ICommunicationWorkflowClient
{
    Task SendQuotationAsync(Guid tenantId, QuotationCommunicationRequest request, CancellationToken cancellationToken);
    Task SendItineraryAsync(Guid tenantId, ItineraryCommunicationRequest request, CancellationToken cancellationToken);
}

public sealed record CommunicationDocumentReference(
    string Name,
    string? DocumentId,
    string? Url,
    string? ContentType,
    long? SizeBytes,
    Dictionary<string, string>? Metadata);

public sealed record QuotationCommunicationRequest(
    Guid RecipientId,
    string Channel,
    string Subject,
    string Body,
    string ReferenceId,
    string CorrelationId,
    string IdempotencyKey,
    IReadOnlyList<CommunicationDocumentReference> Documents);

public sealed record ItineraryCommunicationRequest(
    Guid RecipientId,
    string Channel,
    string Subject,
    string Body,
    string ReferenceId,
    string CorrelationId,
    string IdempotencyKey,
    IReadOnlyList<CommunicationDocumentReference> Documents);
