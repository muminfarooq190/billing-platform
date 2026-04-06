using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelService.Domain.Aggregates;

namespace TravelService.Infrastructure.Persistence.Configurations;

public sealed class QuotationAttachmentConfiguration : IEntityTypeConfiguration<QuotationAttachment>
{
    public void Configure(EntityTypeBuilder<QuotationAttachment> builder)
    {
        builder.ToTable("quotation_attachments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.QuotationId).HasColumnName("quotation_id");
        builder.Property(x => x.QuotationRevisionId).HasColumnName("quotation_revision_id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.StorageKey).HasColumnName("storage_key").HasMaxLength(1024);
        builder.Property(x => x.OriginalFileName).HasColumnName("original_file_name").HasMaxLength(512);
        builder.Property(x => x.ContentType).HasColumnName("content_type").HasMaxLength(256);
        builder.Property(x => x.SizeBytes).HasColumnName("size_bytes");
        builder.Property(x => x.AttachmentType).HasColumnName("attachment_type").HasMaxLength(64);
        builder.Property(x => x.Caption).HasColumnName("caption");
        builder.Property(x => x.IsCustomerVisible).HasColumnName("is_customer_visible");
        builder.Property(x => x.SortOrder).HasColumnName("sort_order");
        builder.Property(x => x.UploadedByUserId).HasColumnName("uploaded_by_user_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => new { x.TenantId, x.QuotationId, x.SortOrder });
        builder.HasOne<Quotation>().WithMany().HasForeignKey(x => x.QuotationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<QuotationRevision>().WithMany().HasForeignKey(x => x.QuotationRevisionId).OnDelete(DeleteBehavior.SetNull);
    }
}
