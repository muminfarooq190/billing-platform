using Dapper;
using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Queries.ListBookingItems;

public sealed class ListBookingItemsQueryHandler(
    IReadDbConnectionFactory connectionFactory,
    IBookingRepository bookingRepository) : IRequestHandler<ListBookingItemsQuery, IReadOnlyList<BookingItemReadModel>>
{
    public async Task<IReadOnlyList<BookingItemReadModel>> Handle(ListBookingItemsQuery request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new DomainException($"Booking {request.BookingId} not found.");

        if (booking.TenantId != request.TenantId)
            throw new DomainException("Booking does not belong to the active tenant.");

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;
        var items = await dbConnection.QueryAsync<BookingItemReadModel>(
            @"SELECT id,
                      booking_id AS BookingId,
                      tenant_id AS TenantId,
                      type,
                      status,
                      supplier_name AS SupplierName,
                      supplier_reference AS SupplierReference,
                      title,
                      description,
                      location,
                      start_at AS StartAt,
                      end_at AS EndAt,
                      sell_amount AS SellAmount,
                      cost_amount AS CostAmount,
                      currency,
                      voucher_number AS VoucherNumber,
                      confirmation_number AS ConfirmationNumber,
                      assigned_to_user_id AS AssignedToUserId,
                      notes,
                      sort_order AS SortOrder,
                      created_at AS CreatedAt,
                      updated_at AS UpdatedAt
               FROM booking_items
               WHERE booking_id = @BookingId AND tenant_id = @TenantId AND deleted_at IS NULL
               ORDER BY sort_order ASC, created_at ASC",
            new { request.BookingId, request.TenantId });

        return items.ToList();
    }
}
