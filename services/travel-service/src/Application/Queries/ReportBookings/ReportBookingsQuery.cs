using MediatR;

namespace TravelService.Application.Queries.ReportBookings;

public sealed record ReportBookingsQuery(Guid TenantId, string? Status = null, string? Destination = null) : IRequest<IReadOnlyList<BookingReportRow>>;

public sealed record BookingReportRow(
    Guid BookingId,
    string BookingNumber,
    string Title,
    string Destination,
    string Status,
    string Currency,
    decimal TotalSellAmount,
    DateTimeOffset TravelDate,
    DateTimeOffset ReturnDate,
    int Travellers);
