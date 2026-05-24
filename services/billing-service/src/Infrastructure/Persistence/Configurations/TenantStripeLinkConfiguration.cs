using BillingService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillingService.Infrastructure.Persistence.Configurations;

public sealed class TenantStripeLinkConfiguration : IEntityTypeConfiguration<TenantStripeLink>
{
    public void Configure(EntityTypeBuilder<TenantStripeLink> builder)
    {
        builder.ToTable("tenant_stripe_links");
        builder.HasKey(x => x.TenantId);
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.StripeCustomerId).HasColumnName("stripe_customer_id").HasMaxLength(64).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.HasIndex(x => x.StripeCustomerId).IsUnique();
    }
}
