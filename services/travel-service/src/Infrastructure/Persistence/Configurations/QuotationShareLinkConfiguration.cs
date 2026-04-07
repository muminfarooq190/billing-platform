using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelService.Domain.Aggregates;

namespace TravelService.Infrastructure.Persistence.Configurations;

public sealed class QuotationShareLinkConfiguration : IEntityTypeConfiguration<QuotationShareLink>
{
    public void Configure(EntityTypeBuilder<QuotationShareLink> builder)
    {
        builder.ToTable("quotation_share_links");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.QuotationId).HasColumnName("quotation_id");
        builder.Property(x => x.QuotationRevisionId).HasColumnName("quotation_revision_id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.Token).HasColumnName("token").HasMaxLength(256);
        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at");
        builder.Property(x => x.RevokedAt).HasColumnName("revoked_at");
        builder.Property(x => x.LastViewedAt).HasColumnName("last_viewed_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(x => x.Token).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.QuotationId });
    }
}
