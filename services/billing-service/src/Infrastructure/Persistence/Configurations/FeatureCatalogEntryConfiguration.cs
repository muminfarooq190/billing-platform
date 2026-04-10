using BillingService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillingService.Infrastructure.Persistence.Configurations;

public sealed class FeatureCatalogEntryConfiguration : IEntityTypeConfiguration<FeatureCatalogEntry>
{
    public void Configure(EntityTypeBuilder<FeatureCatalogEntry> builder)
    {
        builder.ToTable("feature_catalog");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FeatureKey).HasColumnName("feature_key").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Service).HasColumnName("service").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Category).HasColumnName("category").HasMaxLength(100).IsRequired();
        builder.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(1000).IsRequired();
        builder.Property(x => x.IsQuota).HasColumnName("is_quota");
        builder.Property(x => x.Unit).HasColumnName("unit").HasMaxLength(100);
        builder.Property(x => x.MetadataJson).HasColumnName("metadata_json").HasColumnType("jsonb");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.FeatureKey).IsUnique();
    }
}
