using Dapper;
using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Queries.ListTravelersByBooking;

public sealed class ListTravelersByBookingQueryHandler(
    IReadDbConnectionFactory connectionFactory,
    IBookingRepository bookingRepository) : IRequestHandler<ListTravelersByBookingQuery, IReadOnlyList<TravelerReadModel>>
{
    public async Task<IReadOnlyList<TravelerReadModel>> Handle(ListTravelersByBookingQuery request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new DomainException($"Booking {request.BookingId} not found.");

        if (booking.TenantId != request.TenantId)
            throw new DomainException("Booking does not belong to the active tenant.");

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;
        var travelers = await dbConnection.QueryAsync<TravelerReadModel>(
            @"SELECT id,
                      booking_id AS BookingId,
                      tenant_id AS TenantId,
                      first_name AS FirstName,
                      last_name AS LastName,
                      date_of_birth AS DateOfBirth,
                      gender,
                      email,
                      phone,
                      passport_number AS PassportNumber,
                      passport_expiry AS PassportExpiry,
                      nationality,
                      meal_preference AS MealPreference,
                      special_assistance_notes AS SpecialAssistanceNotes,
                      emergency_contact_name AS EmergencyContactName,
                      emergency_contact_phone AS EmergencyContactPhone,
                      lead_traveler AS LeadTraveler,
                      created_at AS CreatedAt,
                      updated_at AS UpdatedAt
               FROM travelers
               WHERE booking_id = @BookingId AND tenant_id = @TenantId AND deleted_at IS NULL
               ORDER BY created_at ASC",
            new { request.BookingId, request.TenantId });

        return travelers.ToList();
    }
}
