using MediatR;

namespace TravelService.Application.Queries.GetQuotationById;

public sealed record GetQuotationByIdQuery(Guid Id) : IRequest<QuotationReadModel?>;

public sealed class QuotationReadModel
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid CustomerContactId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Destination { get; init; } = string.Empty;
    public DateTimeOffset TravelDate { get; init; }
    public DateTimeOffset ReturnDate { get; init; }
    public int Travellers { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset ValidUntil { get; init; }
    public int CurrentRevisionNumber { get; init; }
    public Guid? AcceptedRevisionId { get; init; }
    public decimal TotalAmount { get; init; }
    public int AttachmentCount { get; init; }
    public bool HasCustomerVisibleAttachments { get; init; }
    public DateTimeOffset? LastSentAt { get; init; }
    public DateTimeOffset? LastViewedAt { get; init; }
    public DateTimeOffset? ExpiredAt { get; init; }
    public DateTimeOffset? RejectedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
