using BillingService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillingService.Infrastructure.Persistence.Configurations;

public sealed class TenantSubscriptionPackageConfiguration : IEntityTypeConfiguration<TenantSubscriptionPackage>
{
    public void Configure(EntityTypeBuilder<TenantSubscriptionPackage> builder)
    {
        builder.ToTable("tenant_subscription_packages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.CommercialPackageId).HasColumnName("commercial_package_id");
        builder.Property(x => x.Source).HasColumnName("source").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(100).IsRequired();
        builder.Property(x => x.EffectiveFrom).HasColumnName("effective_from");
        builder.Property(x => x.EffectiveTo).HasColumnName("effective_to");
        builder.Property(x => x.MetadataJson).HasColumnName("metadata_json").HasColumnType("jsonb");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => new { x.TenantId, x.CommercialPackageId, x.DeletedAt });
    }
}
