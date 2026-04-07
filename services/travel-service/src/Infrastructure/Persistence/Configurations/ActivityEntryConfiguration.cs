using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelService.Domain.Aggregates;

namespace TravelService.Infrastructure.Persistence.Configurations;

public sealed class ActivityEntryConfiguration : IEntityTypeConfiguration<ActivityEntry>
{
    public void Configure(EntityTypeBuilder<ActivityEntry> builder)
    {
        builder.ToTable("activity_entries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.EntityType).HasColumnName("entity_type");
        builder.Property(x => x.EntityId).HasColumnName("entity_id");
        builder.Property(x => x.ActivityType).HasColumnName("activity_type");
        builder.Property(x => x.Summary).HasColumnName("summary");
        builder.Property(x => x.DetailJson).HasColumnName("detail_json").HasColumnType("jsonb");
        builder.Property(x => x.ActorUserId).HasColumnName("actor_user_id");
        builder.Property(x => x.OccurredAt).HasColumnName("occurred_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId, x.OccurredAt });
    }
}
