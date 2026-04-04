using CommunicationService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunicationService.Infrastructure.Persistence.Configurations;

public sealed class RecipientPreferencesConfiguration : IEntityTypeConfiguration<RecipientPreferences>
{
    public void Configure(EntityTypeBuilder<RecipientPreferences> builder)
    {
        builder.ToTable("recipient_preferences");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.RecipientId).HasColumnName("recipient_id");
        builder.Property(x => x.RecipientType).HasConversion<string>().HasColumnName("recipient_type");
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(256);
        builder.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(32);
        builder.Property(x => x.DeviceToken).HasColumnName("device_token").HasMaxLength(512);
        builder.Property(x => x.Timezone).HasColumnName("timezone").HasMaxLength(64);
        builder.Property(x => x.ChannelPreferencesJson).HasColumnName("channel_preferences_json");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Ignore(x => x.ChannelPreferences);
        builder.HasIndex(x => new { x.RecipientId, x.TenantId }).IsUnique();
    }
}
