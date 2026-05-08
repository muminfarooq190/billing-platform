using IdentityService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public sealed class WorkflowStageConfiguration : IEntityTypeConfiguration<WorkflowStage>
{
    public void Configure(EntityTypeBuilder<WorkflowStage> builder)
    {
        builder.ToTable("workflow_stages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.Key).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Label).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Color).HasMaxLength(16).IsRequired();
        builder.Property(x => x.Icon).HasMaxLength(80).IsRequired();
        builder.Property(x => x.SortOrder).HasColumnName("sort_order");
        builder.Property(x => x.Required).HasColumnName("required");
        builder.Property(x => x.TemplateContext).HasColumnName("template_context").HasMaxLength(60);
        builder.Property(x => x.AutomationType).HasColumnName("automation_type").HasMaxLength(60).IsRequired();
        builder.Property(x => x.AutomationPayloadJson)
            .HasColumnName("automation_payload_json")
            .HasColumnType("jsonb")
            .IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.SortOrder });
    }
}
