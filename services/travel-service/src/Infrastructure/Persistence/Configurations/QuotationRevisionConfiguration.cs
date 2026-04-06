using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelService.Domain.Aggregates;

namespace TravelService.Infrastructure.Persistence.Configurations;

public sealed class QuotationRevisionConfiguration : IEntityTypeConfiguration<QuotationRevision>
{
    public void Configure(EntityTypeBuilder<QuotationRevision> builder)
    {
        builder.ToTable("quotation_revisions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.QuotationId).HasColumnName("quotation_id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.RevisionNumber).HasColumnName("revision_number");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(64);
        builder.Property(x => x.CustomerContactId).HasColumnName("customer_contact_id");
        builder.Property(x => x.CustomerName).HasColumnName("customer_name").HasMaxLength(256);
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(512);
        builder.Property(x => x.Destination).HasColumnName("destination").HasMaxLength(512);
        builder.Property(x => x.TravelDate).HasColumnName("travel_date");
        builder.Property(x => x.ReturnDate).HasColumnName("return_date");
        builder.Property(x => x.Travellers).HasColumnName("travellers");
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
        builder.Property(x => x.Notes).HasColumnName("notes");
        builder.Property(x => x.VisibleNotes).HasColumnName("visible_notes");
        builder.Property(x => x.InternalNotes).HasColumnName("internal_notes");
        builder.Property(x => x.ValidUntil).HasColumnName("valid_until");
        builder.Property(x => x.SubtotalAmount).HasColumnName("subtotal_amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.TaxAmount).HasColumnName("tax_amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(x => new { x.QuotationId, x.RevisionNumber }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.QuotationId, x.RevisionNumber });

        builder.HasOne<Quotation>()
            .WithMany()
            .HasForeignKey(x => x.QuotationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.LineItems).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(x => x.LineItems)
            .WithOne()
            .HasForeignKey(x => x.QuotationRevisionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
