using MediatR;

namespace TravelService.Application.Commands.AddBookingItem;

public sealed record AddBookingItemCommand(
    Guid TenantId,
    Guid BookingId,
    string Type,
    string Title,
    string? Description,
    string SupplierName,
    string? SupplierReference,
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
    int SortOrder) : IRequest<Guid>;
