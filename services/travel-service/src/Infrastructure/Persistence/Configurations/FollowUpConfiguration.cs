using TravelService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelService.Infrastructure.Persistence.Configurations;

public sealed class FollowUpConfiguration : IEntityTypeConfiguration<FollowUp>
{
    public void Configure(EntityTypeBuilder<FollowUp> builder)
    {
        builder.ToTable("follow_ups");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.CustomerContactId).HasColumnName("customer_contact_id");
        builder.Property(x => x.CustomerName).HasColumnName("customer_name").HasMaxLength(256);
        builder.Property(x => x.Subject).HasColumnName("subject").HasMaxLength(512);
        builder.Property(x => x.Notes).HasColumnName("notes");
        builder.Property(x => x.Priority).HasConversion<string>().HasColumnName("priority");
        builder.Property(x => x.Status).HasConversion<string>().HasColumnName("status");
        builder.Property(x => x.DueDate).HasColumnName("due_date");
        builder.Property(x => x.AssignedToUserId).HasColumnName("assigned_to_user_id");
        builder.Property(x => x.CompletedAt).HasColumnName("completed_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.DueDate);
    }
}
