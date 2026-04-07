using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelService.Domain.Aggregates;

namespace TravelService.Infrastructure.Persistence.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.QuotationId).HasColumnName("quotation_id");
        builder.Property(x => x.AcceptedRevisionId).HasColumnName("accepted_revision_id");
        builder.Property(x => x.PrimaryContactId).HasColumnName("primary_contact_id");
        builder.Property(x => x.BookingNumber).HasColumnName("booking_number").HasMaxLength(64);
        builder.Property(x => x.Status).HasConversion<string>().HasColumnName("status");
        builder.Property(x => x.TripName).HasColumnName("trip_name").HasMaxLength(512);
        builder.Property(x => x.Destination).HasColumnName("destination").HasMaxLength(512);
        builder.Property(x => x.StartDate).HasColumnName("start_date");
        builder.Property(x => x.EndDate).HasColumnName("end_date");
        builder.Property(x => x.TravellersCount).HasColumnName("travellers_count");
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
        builder.Property(x => x.TotalSellAmount).HasColumnName("total_sell_amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalCostAmount).HasColumnName("total_cost_amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.MarginAmount).HasColumnName("margin_amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.AssignedToUserId).HasColumnName("assigned_to_user_id");
        builder.Property(x => x.CustomerReference).HasColumnName("customer_reference");
        builder.Property(x => x.InternalNotes).HasColumnName("internal_notes");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.CancelledAt).HasColumnName("cancelled_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.BookingNumber).IsUnique();
        builder.HasIndex(x => x.AcceptedRevisionId).IsUnique();
    }
}
