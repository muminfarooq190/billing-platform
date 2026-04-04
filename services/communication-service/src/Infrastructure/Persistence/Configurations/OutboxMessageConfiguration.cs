using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunicationService.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.AggregateType).HasColumnName("aggregate_type").HasMaxLength(256);
        builder.Property(x => x.AggregateId).HasColumnName("aggregate_id");
        builder.Property(x => x.EventType).HasColumnName("event_type").HasMaxLength(256);
        builder.Property(x => x.Payload).HasColumnName("payload");
        builder.Property(x => x.PublishedAt).HasColumnName("published_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(x => x.PublishedAt);
    }
}
