using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelService.Domain.Aggregates;

namespace TravelService.Infrastructure.Persistence.Configurations;

public sealed class ItineraryConfiguration : IEntityTypeConfiguration<Itinerary>
{
    public void Configure(EntityTypeBuilder<Itinerary> builder)
    {
        builder.ToTable("itineraries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.CustomerContactId).HasColumnName("customer_contact_id");
        builder.Property(x => x.CustomerName).HasColumnName("customer_name").HasMaxLength(256);
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(512);
        builder.Property(x => x.Destination).HasColumnName("destination").HasMaxLength(512);
        builder.Property(x => x.StartDate).HasColumnName("start_date");
        builder.Property(x => x.EndDate).HasColumnName("end_date");
        builder.Property(x => x.Travellers).HasColumnName("travellers");
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
        builder.Property(x => x.QuotationId).HasColumnName("quotation_id");
        builder.Property(x => x.BookingId).HasColumnName("booking_id");
        builder.Property(x => x.Status).HasConversion<string>().HasColumnName("status");
        builder.Ignore(x => x.TotalCost);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.QuotationId);
        builder.HasIndex(x => x.BookingId);

        builder.Navigation(x => x.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.OwnsMany(x => x.Items, items =>
        {
            items.ToTable("itinerary_items");
            items.WithOwner().HasForeignKey("itinerary_id");
            items.Property<Guid>("id").HasColumnName("id");
            items.HasKey("id");
            items.Property(x => x.DayNumber).HasColumnName("day_number");
            items.Property(x => x.ItemType).HasConversion<string>().HasColumnName("item_type").HasMaxLength(64);
            items.Property(x => x.Title).HasColumnName("title").HasMaxLength(512);
            items.Property(x => x.Description).HasColumnName("description");
            items.Property(x => x.Location).HasColumnName("location").HasMaxLength(512);
            items.Property(x => x.StartTime).HasColumnName("start_time");
            items.Property(x => x.EndTime).HasColumnName("end_time");
            items.Property(x => x.Cost).HasColumnName("cost").HasColumnType("decimal(18,2)");
            items.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
        });
    }
}
