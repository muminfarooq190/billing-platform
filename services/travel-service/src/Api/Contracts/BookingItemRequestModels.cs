namespace TravelService.Api.Contracts;

public sealed record AddBookingItemRequest(
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
    int SortOrder);

public sealed record UpdateBookingItemRequest(
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
    int SortOrder);

public sealed record UpdateBookingItemStatusRequest(string Status);
