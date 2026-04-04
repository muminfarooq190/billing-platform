using TravelService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelService.Infrastructure.Persistence.Configurations;

public sealed class QuotationConfiguration : IEntityTypeConfiguration<Quotation>
{
    public void Configure(EntityTypeBuilder<Quotation> builder)
    {
        builder.ToTable("quotations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.CustomerContactId).HasColumnName("customer_contact_id");
        builder.Property(x => x.CustomerName).HasColumnName("customer_name").HasMaxLength(256);
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(512);
        builder.Property(x => x.Destination).HasColumnName("destination").HasMaxLength(512);
        builder.Property(x => x.TravelDate).HasColumnName("travel_date");
        builder.Property(x => x.ReturnDate).HasColumnName("return_date");
        builder.Property(x => x.Travellers).HasColumnName("travellers");
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
        builder.Property(x => x.Notes).HasColumnName("notes");
        builder.Property(x => x.Status).HasConversion<string>().HasColumnName("status");
        builder.Property(x => x.ValidUntil).HasColumnName("valid_until");
        builder.Property(x => x.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Ignore(x => x.LineItems);
        builder.HasIndex(x => x.TenantId);
    }
}
