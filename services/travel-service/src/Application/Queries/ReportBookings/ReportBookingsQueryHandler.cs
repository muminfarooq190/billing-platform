using Dapper;
using MediatR;
using TravelService.Application.Abstractions;

namespace TravelService.Application.Queries.ReportBookings;

public sealed class ReportBookingsQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<ReportBookingsQuery, IReadOnlyList<BookingReportRow>>
{
    public async Task<IReadOnlyList<BookingReportRow>> Handle(ReportBookingsQuery request, CancellationToken cancellationToken)
    {
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<BookingReportRow>(@"
            SELECT id AS BookingId,
                   booking_number AS BookingNumber,
                   title,
                   destination,
                   status,
                   currency,
                   total_sell_amount AS TotalSellAmount,
                   travel_date AS TravelDate,
                   return_date AS ReturnDate,
                   travellers
            FROM bookings
            WHERE tenant_id = @TenantId
              AND deleted_at IS NULL
              AND (@Status IS NULL OR status = @Status)
              AND (@Destination IS NULL OR destination ILIKE @Destination)
            ORDER BY travel_date DESC", new
        {
            request.TenantId,
            request.Status,
            Destination = request.Destination is null ? null : $"%{request.Destination}%"
        });

        return rows.ToList();
    }
}
