using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelService.Domain.Aggregates;

namespace TravelService.Infrastructure.Persistence.Configurations;

public sealed class DraftTripConceptConfiguration : IEntityTypeConfiguration<DraftTripConcept>
{
    public void Configure(EntityTypeBuilder<DraftTripConcept> builder)
    {
        builder.ToTable("draft_trip_concepts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.TravelInquiryId).HasColumnName("travel_inquiry_id");
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(256);
        builder.Property(x => x.Destination).HasColumnName("destination").HasMaxLength(256);
        builder.Property(x => x.Summary).HasColumnName("summary");
        builder.Property(x => x.StartDate).HasColumnName("start_date");
        builder.Property(x => x.EndDate).HasColumnName("end_date");
        builder.Property(x => x.Travellers).HasColumnName("travellers");
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
        builder.Property(x => x.BudgetAmount).HasColumnName("budget_amount").HasColumnType("numeric(18,2)");
        builder.Property(x => x.ConceptStatus).HasConversion<string>().HasColumnName("concept_status").HasMaxLength(64);
        builder.Property(x => x.IsPrimary).HasColumnName("is_primary");
        builder.Property(x => x.OptionLabel).HasColumnName("option_label").HasMaxLength(128);
        builder.Property(x => x.Notes).HasColumnName("notes");
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => new { x.TenantId, x.TravelInquiryId, x.IsPrimary });
        builder.HasIndex(x => new { x.TenantId, x.ConceptStatus, x.UpdatedAt });

        builder.OwnsMany(x => x.Days, days =>
        {
            days.ToTable("draft_trip_concept_days");
            days.WithOwner().HasForeignKey("draft_trip_concept_id");
            days.Property<Guid>("Id").HasColumnName("id");
            days.HasKey("Id");
            days.Property(x => x.DayNumber).HasColumnName("day_number");
            days.Property(x => x.Title).HasColumnName("title").HasMaxLength(256);
            days.Property(x => x.Description).HasColumnName("description");
            days.Property(x => x.Location).HasColumnName("location").HasMaxLength(256);
            days.Property(x => x.OvernightLocation).HasColumnName("overnight_location").HasMaxLength(256);
            days.Property(x => x.CreatedAt).HasColumnName("created_at");
        });
    }
}
