using IdentityService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public sealed class TenantBrandingConfiguration : IEntityTypeConfiguration<TenantBranding>
{
    public void Configure(EntityTypeBuilder<TenantBranding> builder)
    {
        builder.ToTable("tenant_branding");
        builder.HasKey(x => x.TenantId);
        builder.Property(x => x.DisplayName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.LegalName).HasMaxLength(255);
        builder.Property(x => x.PrimaryColor).HasMaxLength(32).IsRequired();
        builder.Property(x => x.SecondaryColor).HasMaxLength(32).IsRequired();
        builder.Property(x => x.AccentColor).HasMaxLength(32).IsRequired();
        builder.Property(x => x.TextColor).HasMaxLength(32).IsRequired();
        builder.Property(x => x.BackgroundColor).HasMaxLength(32).IsRequired();
        builder.Property(x => x.ThemeMode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.DefaultFontFamily).HasMaxLength(100).IsRequired();
        builder.Property(x => x.SupportEmail).HasMaxLength(255);
        builder.Property(x => x.SupportPhone).HasMaxLength(100);
        builder.Property(x => x.WebsiteUrl).HasMaxLength(500);
        builder.Property(x => x.Tagline).HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}
