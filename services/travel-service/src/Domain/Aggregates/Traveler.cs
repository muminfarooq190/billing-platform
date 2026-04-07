using TravelService.Domain.Exceptions;

namespace TravelService.Domain.Aggregates;

public sealed class Traveler
{
    private Traveler() { }

    private Traveler(Guid bookingId, Guid tenantId, string firstName, string lastName, DateOnly? dateOfBirth, string? gender, string? email, string? phone, string? passportNumber, DateOnly? passportExpiry, string? nationality, string? mealPreference, string? specialAssistanceNotes, string? emergencyContactName, string? emergencyContactPhone, bool leadTraveler)
    {
        if (bookingId == Guid.Empty)
            throw new DomainException("BookingId is required.");
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId is required.");
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required.");
        if (passportExpiry.HasValue && passportExpiry.Value <= DateOnly.FromDateTime(DateTime.UtcNow.Date))
            throw new DomainException("Passport expiry must be in the future.");

        Id = Guid.NewGuid();
        BookingId = bookingId;
        TenantId = tenantId;
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        DateOfBirth = dateOfBirth;
        Gender = NormalizeOptional(gender);
        Email = NormalizeOptional(email);
        Phone = NormalizeOptional(phone);
        PassportNumber = NormalizeOptional(passportNumber);
        PassportExpiry = passportExpiry;
        Nationality = NormalizeOptional(nationality);
        MealPreference = NormalizeOptional(mealPreference);
        SpecialAssistanceNotes = NormalizeOptional(specialAssistanceNotes);
        EmergencyContactName = NormalizeOptional(emergencyContactName);
        EmergencyContactPhone = NormalizeOptional(emergencyContactPhone);
        LeadTraveler = leadTraveler;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid BookingId { get; private set; }
    public Guid TenantId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly? DateOfBirth { get; private set; }
    public string? Gender { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? PassportNumber { get; private set; }
    public DateOnly? PassportExpiry { get; private set; }
    public string? Nationality { get; private set; }
    public string? MealPreference { get; private set; }
    public string? SpecialAssistanceNotes { get; private set; }
    public string? EmergencyContactName { get; private set; }
    public string? EmergencyContactPhone { get; private set; }
    public bool LeadTraveler { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static Traveler Create(Guid bookingId, Guid tenantId, string firstName, string lastName, DateOnly? dateOfBirth, string? gender, string? email, string? phone, string? passportNumber, DateOnly? passportExpiry, string? nationality, string? mealPreference, string? specialAssistanceNotes, string? emergencyContactName, string? emergencyContactPhone, bool leadTraveler)
        => new(bookingId, tenantId, firstName, lastName, dateOfBirth, gender, email, phone, passportNumber, passportExpiry, nationality, mealPreference, specialAssistanceNotes, emergencyContactName, emergencyContactPhone, leadTraveler);

    public void Update(string firstName, string lastName, DateOnly? dateOfBirth, string? gender, string? email, string? phone, string? passportNumber, DateOnly? passportExpiry, string? nationality, string? mealPreference, string? specialAssistanceNotes, string? emergencyContactName, string? emergencyContactPhone, bool leadTraveler)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required.");
        if (passportExpiry.HasValue && passportExpiry.Value <= DateOnly.FromDateTime(DateTime.UtcNow.Date))
            throw new DomainException("Passport expiry must be in the future.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        DateOfBirth = dateOfBirth;
        Gender = NormalizeOptional(gender);
        Email = NormalizeOptional(email);
        Phone = NormalizeOptional(phone);
        PassportNumber = NormalizeOptional(passportNumber);
        PassportExpiry = passportExpiry;
        Nationality = NormalizeOptional(nationality);
        MealPreference = NormalizeOptional(mealPreference);
        SpecialAssistanceNotes = NormalizeOptional(specialAssistanceNotes);
        EmergencyContactName = NormalizeOptional(emergencyContactName);
        EmergencyContactPhone = NormalizeOptional(emergencyContactPhone);
        LeadTraveler = leadTraveler;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Delete()
    {
        if (DeletedAt is not null)
            throw new DomainException("Traveler is already deleted.");

        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
