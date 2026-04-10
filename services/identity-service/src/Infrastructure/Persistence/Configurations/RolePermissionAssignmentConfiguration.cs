using IdentityService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public sealed class RolePermissionAssignmentConfiguration : IEntityTypeConfiguration<RolePermissionAssignment>
{
    public void Configure(EntityTypeBuilder<RolePermissionAssignment> builder)
    {
        builder.ToTable("role_permission_assignments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PermissionKey).HasMaxLength(150).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(x => new { x.RoleDefinitionId, x.PermissionKey }).IsUnique();
    }
}
