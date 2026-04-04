using CommunicationService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunicationService.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.RecipientId).HasColumnName("recipient_id");
        builder.Property(x => x.RecipientType).HasConversion<string>().HasColumnName("recipient_type");
        builder.Property(x => x.Channel).HasConversion<string>().HasColumnName("channel");
        builder.Property(x => x.Subject).HasColumnName("subject").HasMaxLength(512);
        builder.Property(x => x.Body).HasColumnName("body");
        builder.Property(x => x.Priority).HasConversion<string>().HasColumnName("priority");
        builder.Property(x => x.Status).HasConversion<string>().HasColumnName("status");
        builder.Property(x => x.TemplateId).HasColumnName("template_id");
        builder.Property(x => x.ReferenceId).HasColumnName("reference_id").HasMaxLength(256);
        builder.Property(x => x.RetryCount).HasColumnName("retry_count");
        builder.Property(x => x.LastError).HasColumnName("last_error");
        builder.Property(x => x.ProviderMessageId).HasColumnName("provider_message_id").HasMaxLength(256);
        builder.Property(x => x.SentAt).HasColumnName("sent_at");
        builder.Property(x => x.DeliveredAt).HasColumnName("delivered_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.HasIndex(x => x.RecipientId);
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.Status);
    }
}
