using IdentityService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public sealed class TenantSettingsConfiguration : IEntityTypeConfiguration<TenantSettings>
{
    public void Configure(EntityTypeBuilder<TenantSettings> builder)
    {
        builder.ToTable("tenant_settings");
        builder.HasKey(x => x.TenantId);
        builder.Property(x => x.Timezone).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Locale).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DateFormat).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        builder.Property(x => x.NumberFormat).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DefaultCountry).HasMaxLength(100).IsRequired();
        builder.Property(x => x.SettingsJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}
