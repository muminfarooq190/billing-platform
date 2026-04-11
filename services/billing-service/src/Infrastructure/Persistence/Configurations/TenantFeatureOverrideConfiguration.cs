using BillingService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillingService.Infrastructure.Persistence.Configurations;

public sealed class TenantFeatureOverrideConfiguration : IEntityTypeConfiguration<TenantFeatureOverride>
{
    public void Configure(EntityTypeBuilder<TenantFeatureOverride> builder)
    {
        builder.ToTable("tenant_feature_overrides");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.FeatureKey).HasColumnName("feature_key").HasMaxLength(200);
        builder.Property(x => x.Granted).HasColumnName("granted");
        builder.Property(x => x.LimitValue).HasColumnName("limit_value");
        builder.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(500);
        builder.Property(x => x.Source).HasColumnName("source").HasMaxLength(100);
        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(200);
        builder.Property(x => x.EffectiveFrom).HasColumnName("effective_from");
        builder.Property(x => x.EffectiveTo).HasColumnName("effective_to");
        builder.Property(x => x.MetadataJson).HasColumnName("metadata_json").HasColumnType("jsonb");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => new { x.TenantId, x.FeatureKey, x.DeletedAt });
    }
}
