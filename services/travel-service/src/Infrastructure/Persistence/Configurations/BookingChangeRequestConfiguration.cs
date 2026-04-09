using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelService.Domain.Aggregates;

namespace TravelService.Infrastructure.Persistence.Configurations;

public sealed class BookingChangeRequestConfiguration : IEntityTypeConfiguration<BookingChangeRequest>
{
    public void Configure(EntityTypeBuilder<BookingChangeRequest> builder)
    {
        builder.ToTable("booking_change_requests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BookingId).HasColumnName("booking_id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.ChangeType).HasColumnName("change_type");
        builder.Property(x => x.Reason).HasColumnName("reason");
        builder.Property(x => x.Status).HasColumnName("status");
        builder.Property(x => x.RequestedByUserId).HasColumnName("requested_by_user_id");
        builder.Property(x => x.ReviewedByUserId).HasColumnName("reviewed_by_user_id");
        builder.Property(x => x.DecisionReason).HasColumnName("decision_reason");
        builder.Property(x => x.RequestedAt).HasColumnName("requested_at");
        builder.Property(x => x.ReviewedAt).HasColumnName("reviewed_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.HasIndex(x => new { x.BookingId, x.Status, x.RequestedAt });
    }
}
