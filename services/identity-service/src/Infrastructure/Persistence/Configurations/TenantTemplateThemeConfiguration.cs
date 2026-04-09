using IdentityService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public sealed class TenantTemplateThemeConfiguration : IEntityTypeConfiguration<TenantTemplateTheme>
{
    public void Configure(EntityTypeBuilder<TenantTemplateTheme> builder)
    {
        builder.ToTable("tenant_template_themes");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.TenantId, x.TemplateScope }).IsUnique();
        builder.Property(x => x.TemplateScope).HasMaxLength(100).IsRequired();
        builder.Property(x => x.HeaderHtml).HasColumnType("text");
        builder.Property(x => x.FooterHtml).HasColumnType("text");
        builder.Property(x => x.CustomCss).HasColumnType("text");
        builder.Property(x => x.SettingsJson).HasColumnType("jsonb");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}
