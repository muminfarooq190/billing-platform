using IdentityService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserInvitation> UserInvitations => Set<UserInvitation>();
    public DbSet<TenantSettings> TenantSettings => Set<TenantSettings>();
    public DbSet<IdentityAuditLog> IdentityAuditLogs => Set<IdentityAuditLog>();
    public DbSet<SecurityEvent> SecurityEvents => Set<SecurityEvent>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<PermissionDefinition> PermissionDefinitions => Set<PermissionDefinition>();
    public DbSet<RoleDefinition> RoleDefinitions => Set<RoleDefinition>();
    public DbSet<RolePermissionAssignment> RolePermissionAssignments => Set<RolePermissionAssignment>();
    public DbSet<UserRoleAssignment> UserRoleAssignments => Set<UserRoleAssignment>();
    public DbSet<UserMfaEnrollment> UserMfaEnrollments => Set<UserMfaEnrollment>();
    public DbSet<TenantBranding> TenantBranding => Set<TenantBranding>();
    public DbSet<TenantBrandAsset> TenantBrandAssets => Set<TenantBrandAsset>();
    public DbSet<TenantTemplateTheme> TenantTemplateThemes => Set<TenantTemplateTheme>();
    public DbSet<OutboxMessage> DomainEvents => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public string AggregateType { get; set; } = string.Empty;
    public Guid AggregateId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
