using IdentityService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public sealed class IdentityAuditLogConfiguration : IEntityTypeConfiguration<IdentityAuditLog>
{
    public void Configure(EntityTypeBuilder<IdentityAuditLog> builder)
    {
        builder.ToTable("identity_audit_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.BeforeJson).HasColumnType("jsonb");
        builder.Property(x => x.AfterJson).HasColumnType("jsonb");
        builder.Property(x => x.IpAddress).HasMaxLength(100);
        builder.Property(x => x.UserAgent).HasMaxLength(512);
        builder.Property(x => x.OccurredAt).HasColumnName("occurred_at");
        builder.HasIndex(x => new { x.TenantId, x.TargetUserId, x.OccurredAt });
    }
}
