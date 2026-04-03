using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("domain_events");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AggregateType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EventType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Payload).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.PublishedAt).HasColumnName("published_at");
        builder.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_domain_events_unpublished").HasFilter("published_at IS NULL");
    }
}
