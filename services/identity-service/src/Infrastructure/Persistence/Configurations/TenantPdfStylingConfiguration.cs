using IdentityService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public sealed class TenantPdfStylingConfiguration : IEntityTypeConfiguration<TenantPdfStyling>
{
    public void Configure(EntityTypeBuilder<TenantPdfStyling> builder)
    {
        builder.ToTable("tenant_pdf_styling");
        builder.HasKey(x => x.TenantId);
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.HeaderLayout).HasColumnName("header_layout").HasMaxLength(80).IsRequired();
        builder.Property(x => x.FooterText).HasColumnName("footer_text").HasMaxLength(500);
        builder.Property(x => x.WatermarkText).HasColumnName("watermark_text").HasMaxLength(120);
        builder.Property(x => x.AccentColor).HasColumnName("accent_color").HasMaxLength(16).IsRequired();
        builder.Property(x => x.MarginPx).HasColumnName("margin_px");
        builder.Property(x => x.CustomCssJson).HasColumnName("custom_css_json").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}

public sealed class TenantEmailTemplateStyleConfiguration : IEntityTypeConfiguration<TenantEmailTemplateStyle>
{
    public void Configure(EntityTypeBuilder<TenantEmailTemplateStyle> builder)
    {
        builder.ToTable("tenant_email_template_styles");
        builder.HasKey(x => x.TenantId);
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.HeaderHtml).HasColumnName("header_html").HasColumnType("text");
        builder.Property(x => x.FooterHtml).HasColumnName("footer_html").HasColumnType("text");
        builder.Property(x => x.AccentColor).HasColumnName("accent_color").HasMaxLength(16).IsRequired();
        builder.Property(x => x.FontFamily).HasColumnName("font_family").HasMaxLength(200).IsRequired();
        builder.Property(x => x.CustomCssJson).HasColumnName("custom_css_json").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}
