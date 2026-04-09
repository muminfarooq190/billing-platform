using BillingService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillingService.Infrastructure.Persistence.Configurations;

public sealed class FeatureEntitlementConfiguration : IEntityTypeConfiguration<FeatureEntitlement>
{
    public void Configure(EntityTypeBuilder<FeatureEntitlement> builder)
    {
        builder.ToTable("feature_entitlements");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.FeatureKey).HasColumnName("feature_key");
        builder.Property(x => x.Granted).HasColumnName("granted");
        builder.Property(x => x.Source).HasConversion<string>().HasColumnName("source");
        builder.Property(x => x.PlanType).HasConversion<string>().HasColumnName("plan_type");
        builder.Property(x => x.LimitValue).HasColumnName("limit_value");
        builder.Property(x => x.EffectiveFrom).HasColumnName("effective_from");
        builder.Property(x => x.EffectiveTo).HasColumnName("effective_to");
        builder.Property(x => x.MetadataJson).HasColumnName("metadata_json").HasColumnType("jsonb");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => new { x.TenantId, x.FeatureKey });
    }
}
