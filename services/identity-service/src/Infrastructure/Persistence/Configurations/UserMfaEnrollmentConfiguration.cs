using IdentityService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public sealed class UserMfaEnrollmentConfiguration : IEntityTypeConfiguration<UserMfaEnrollment>
{
    public void Configure(EntityTypeBuilder<UserMfaEnrollment> builder)
    {
        builder.ToTable("user_mfa_enrollments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Secret).HasMaxLength(256).IsRequired();
        builder.Property(x => x.RecoveryCodesJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.VerifiedAt).HasColumnName("verified_at");
        builder.Property(x => x.DisabledAt).HasColumnName("disabled_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.HasIndex(x => new { x.TenantId, x.UserId }).IsUnique();
    }
}
