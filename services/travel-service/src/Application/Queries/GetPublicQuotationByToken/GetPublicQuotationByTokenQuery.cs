using MediatR;

namespace TravelService.Application.Queries.GetPublicQuotationByToken;

public sealed record GetPublicQuotationByTokenQuery(string Token) : IRequest<PublicQuotationReadModel?>;

public sealed record PublicQuotationReadModel(
    Guid QuotationId,
    Guid RevisionId,
    string CustomerName,
    string Title,
    string Destination,
    DateTimeOffset TravelDate,
    DateTimeOffset ReturnDate,
    int Travellers,
    string Currency,
    string VisibleNotes,
    DateTimeOffset ValidUntil,
    decimal TotalAmount,
    DateTimeOffset? SentAt,
    DateTimeOffset? LastViewedAt,
    IReadOnlyList<PublicQuotationLineItemReadModel> LineItems,
    IReadOnlyList<PublicQuotationAttachmentReadModel> Attachments);

public sealed record PublicQuotationLineItemReadModel(
    string Description,
    int Quantity,
    decimal UnitPriceAmount,
    string Currency,
    int SortOrder,
    decimal LineTotal);

public sealed record PublicQuotationAttachmentReadModel(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    string AttachmentType,
    string? Caption,
    int SortOrder,
    string ReadUrl);
