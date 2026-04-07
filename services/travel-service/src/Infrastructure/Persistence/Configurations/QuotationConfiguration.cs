using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelService.Domain.Aggregates;

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
        builder.Property(x => x.CurrentRevisionNumber).HasColumnName("current_revision_number");
        builder.Property(x => x.AcceptedRevisionId).HasColumnName("accepted_revision_id");
        builder.Property(x => x.LastSentAt).HasColumnName("last_sent_at");
        builder.Property(x => x.LastViewedAt).HasColumnName("last_viewed_at");
        builder.Property(x => x.ExpiredAt).HasColumnName("expired_at");
        builder.Property(x => x.RejectedAt).HasColumnName("rejected_at");
        builder.Property(x => x.ShareToken).HasColumnName("share_token").HasMaxLength(256);
        builder.Property(x => x.ShareTokenExpiresAt).HasColumnName("share_token_expires_at");
        builder.Ignore(x => x.TotalAmount);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.TenantId);

        builder.Navigation(x => x.LineItems).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.OwnsMany(x => x.LineItems, lineItems =>
        {
            lineItems.ToTable("quotation_line_items");
            lineItems.WithOwner().HasForeignKey("quotation_id");
            lineItems.Property<Guid>("id").HasColumnName("id");
            lineItems.HasKey("id");
            lineItems.Property(x => x.Description).HasColumnName("description").HasMaxLength(512);
            lineItems.Property(x => x.UnitPrice).HasColumnName("unit_price").HasColumnType("decimal(18,2)");
            lineItems.Property(x => x.Quantity).HasColumnName("quantity");
            lineItems.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
            lineItems.Ignore(x => x.Total);
        });
    }
}
