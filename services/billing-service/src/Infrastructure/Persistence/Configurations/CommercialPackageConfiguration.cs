using BillingService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillingService.Infrastructure.Persistence.Configurations;

public sealed class CommercialPackageConfiguration : IEntityTypeConfiguration<CommercialPackage>
{
    public void Configure(EntityTypeBuilder<CommercialPackage> builder)
    {
        builder.ToTable("commercial_packages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Category).HasColumnName("category").HasMaxLength(100).IsRequired();
        builder.Property(x => x.BillingModel).HasColumnName("billing_model").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(1000).IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.MetadataJson).HasColumnName("metadata_json").HasColumnType("jsonb");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.Code).IsUnique();
    }
}
