using IdentityService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public sealed class TenantBrandAssetConfiguration : IEntityTypeConfiguration<TenantBrandAsset>
{
    public void Configure(EntityTypeBuilder<TenantBrandAsset> builder)
    {
        builder.ToTable("tenant_brand_assets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AssetType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.StorageKey).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(128).IsRequired();
        builder.Property(x => x.AltText).HasMaxLength(255);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => new { x.TenantId, x.AssetType, x.IsActive });
    }
}
