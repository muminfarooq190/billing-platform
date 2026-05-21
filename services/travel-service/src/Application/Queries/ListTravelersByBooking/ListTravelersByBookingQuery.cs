using MediatR;

namespace TravelService.Application.Queries.ListTravelersByBooking;

public sealed record ListTravelersByBookingQuery(Guid TenantId, Guid BookingId) : IRequest<IReadOnlyList<TravelerReadModel>>;

/// <summary>
/// Dapper-friendly read model.
/// Switched from positional record → class with init-only properties so Dapper
/// can materialize via parameterless ctor instead of trying to match a strict
/// constructor signature (which fails when column types diverge from the
/// record's signature, e.g. <c>date</c> → <see cref="DateOnly"/>? vs the
/// runtime's <see cref="DateTime"/> mapping).
/// </summary>
public sealed class TravelerReadModel
{
    public Guid Id { get; init; }
    public Guid BookingId { get; init; }
    public Guid TenantId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateOnly? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? PassportNumber { get; init; }
    public DateOnly? PassportExpiry { get; init; }
    public string? Nationality { get; init; }
    public string? MealPreference { get; init; }
    public string? SpecialAssistanceNotes { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
    public bool LeadTraveler { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
