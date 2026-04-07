using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelService.Domain.Aggregates;

namespace TravelService.Infrastructure.Persistence.Configurations;

public sealed class TravelerConfiguration : IEntityTypeConfiguration<Traveler>
{
    public void Configure(EntityTypeBuilder<Traveler> builder)
    {
        builder.ToTable("travelers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.BookingId).HasColumnName("booking_id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(256);
        builder.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(256);
        builder.Property(x => x.DateOfBirth).HasColumnName("date_of_birth");
        builder.Property(x => x.Gender).HasColumnName("gender");
        builder.Property(x => x.Email).HasColumnName("email");
        builder.Property(x => x.Phone).HasColumnName("phone");
        builder.Property(x => x.PassportNumber).HasColumnName("passport_number");
        builder.Property(x => x.PassportExpiry).HasColumnName("passport_expiry");
        builder.Property(x => x.Nationality).HasColumnName("nationality");
        builder.Property(x => x.MealPreference).HasColumnName("meal_preference");
        builder.Property(x => x.SpecialAssistanceNotes).HasColumnName("special_assistance_notes");
        builder.Property(x => x.EmergencyContactName).HasColumnName("emergency_contact_name");
        builder.Property(x => x.EmergencyContactPhone).HasColumnName("emergency_contact_phone");
        builder.Property(x => x.LeadTraveler).HasColumnName("lead_traveler");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => new { x.TenantId, x.BookingId });
    }
}
