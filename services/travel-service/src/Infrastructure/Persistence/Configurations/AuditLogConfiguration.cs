using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelService.Domain.Aggregates;

namespace TravelService.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.EntityType).HasColumnName("entity_type");
        builder.Property(x => x.EntityId).HasColumnName("entity_id");
        builder.Property(x => x.Action).HasColumnName("action");
        builder.Property(x => x.ActorUserId).HasColumnName("actor_user_id");
        builder.Property(x => x.IpAddress).HasColumnName("ip_address");
        builder.Property(x => x.UserAgent).HasColumnName("user_agent");
        builder.Property(x => x.BeforeJson).HasColumnName("before_json").HasColumnType("jsonb");
        builder.Property(x => x.AfterJson).HasColumnName("after_json").HasColumnType("jsonb");
        builder.Property(x => x.MetadataJson).HasColumnName("metadata_json").HasColumnType("jsonb");
        builder.Property(x => x.OccurredAt).HasColumnName("occurred_at");
        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId, x.OccurredAt });
    }
}
