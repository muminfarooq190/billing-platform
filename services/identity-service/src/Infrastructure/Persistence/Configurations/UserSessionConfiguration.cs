using IdentityService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("user_sessions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RefreshTokenId).HasMaxLength(128).IsRequired();
        builder.Property(x => x.DeviceName).HasMaxLength(255);
        builder.Property(x => x.IpAddress).HasMaxLength(100);
        builder.Property(x => x.UserAgent).HasMaxLength(512);
        builder.Property(x => x.LastSeenAt).HasColumnName("last_seen_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.RevokedAt).HasColumnName("revoked_at");
        builder.HasIndex(x => x.RefreshTokenId).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.RevokedAt });
    }
}
