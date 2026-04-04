using BillingService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillingService.Infrastructure.Persistence.Configurations;

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.TenantId).HasColumnName("TenantId");
        builder.Property(x => x.PlanType).HasConversion<string>().HasColumnName("plan_type");
        builder.Property(x => x.BillingCycle).HasConversion<string>().HasColumnName("billing_cycle");
        builder.Property(x => x.Status).HasConversion<string>().HasColumnName("Status");
        builder.Property(x => x.StartDate).HasColumnName("start_date");
        builder.Property(x => x.NextBillingDate).HasColumnName("next_billing_date");
        builder.Property(x => x.CancelledAt).HasColumnName("cancelled_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
    }
}
