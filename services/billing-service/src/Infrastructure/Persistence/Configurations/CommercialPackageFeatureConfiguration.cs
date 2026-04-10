using BillingService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillingService.Infrastructure.Persistence.Configurations;

public sealed class CommercialPackageFeatureConfiguration : IEntityTypeConfiguration<CommercialPackageFeature>
{
    public void Configure(EntityTypeBuilder<CommercialPackageFeature> builder)
    {
        builder.ToTable("commercial_package_features");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CommercialPackageId).HasColumnName("commercial_package_id");
        builder.Property(x => x.FeatureKey).HasColumnName("feature_key").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Granted).HasColumnName("granted");
        builder.Property(x => x.LimitValue).HasColumnName("limit_value");
        builder.Property(x => x.MetadataJson).HasColumnName("metadata_json").HasColumnType("jsonb");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.HasIndex(x => new { x.CommercialPackageId, x.FeatureKey }).IsUnique();
    }
}
