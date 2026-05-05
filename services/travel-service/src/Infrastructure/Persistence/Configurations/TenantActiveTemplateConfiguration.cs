using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelService.Domain.Aggregates;

namespace TravelService.Infrastructure.Persistence.Configurations;

public sealed class TenantActiveTemplateConfiguration : IEntityTypeConfiguration<TenantActiveTemplate>
{
    public void Configure(EntityTypeBuilder<TenantActiveTemplate> builder)
    {
        builder.ToTable("tenant_active_templates");
        builder.HasKey(x => new { x.TenantId, x.Context });
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.Context).HasColumnName("context").HasConversion<string>().IsRequired();
        builder.Property(x => x.TemplateId).HasColumnName("template_id");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}
