using MediatR;

namespace TravelService.Application.Queries.ListBookingItems;

public sealed record ListBookingItemsQuery(Guid TenantId, Guid BookingId) : IRequest<IReadOnlyList<BookingItemReadModel>>;

public sealed record BookingItemReadModel(
    Guid Id,
    Guid BookingId,
    Guid TenantId,
    string Type,
    string Status,
    string SupplierName,
    string? SupplierReference,
    string Title,
    string? Description,
    string? Location,
    DateTimeOffset? StartAt,
    DateTimeOffset? EndAt,
    decimal? SellAmount,
    decimal? CostAmount,
    string? Currency,
    string? VoucherNumber,
    string? ConfirmationNumber,
    Guid? AssignedToUserId,
    string? Notes,
    int SortOrder,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
