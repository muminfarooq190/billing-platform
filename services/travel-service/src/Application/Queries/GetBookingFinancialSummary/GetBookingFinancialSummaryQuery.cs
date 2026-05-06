using MediatR;

namespace TravelService.Application.Queries.GetBookingFinancialSummary;

public sealed record GetBookingFinancialSummaryQuery(Guid TenantId, Guid BookingId) : IRequest<BookingFinancialSummaryReadModel?>;
