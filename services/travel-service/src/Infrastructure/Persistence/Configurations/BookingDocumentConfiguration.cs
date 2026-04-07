using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelService.Domain.Aggregates;

namespace TravelService.Infrastructure.Persistence.Configurations;

public sealed class BookingDocumentConfiguration : IEntityTypeConfiguration<BookingDocument>
{
    public void Configure(EntityTypeBuilder<BookingDocument> builder)
    {
        builder.ToTable("booking_documents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.BookingId).HasColumnName("booking_id");
        builder.Property(x => x.TravelerId).HasColumnName("traveler_id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.StorageKey).HasColumnName("storage_key");
        builder.Property(x => x.OriginalFileName).HasColumnName("original_file_name");
        builder.Property(x => x.ContentType).HasColumnName("content_type");
        builder.Property(x => x.SizeBytes).HasColumnName("size_bytes");
        builder.Property(x => x.DocumentType).HasColumnName("document_type");
        builder.Property(x => x.IsCustomerVisible).HasColumnName("is_customer_visible");
        builder.Property(x => x.Description).HasColumnName("description");
        builder.Property(x => x.UploadedByUserId).HasColumnName("uploaded_by_user_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => new { x.TenantId, x.BookingId });
    }
}
