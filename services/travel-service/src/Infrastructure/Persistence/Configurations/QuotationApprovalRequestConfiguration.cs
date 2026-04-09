using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelService.Domain.Aggregates;

namespace TravelService.Infrastructure.Persistence.Configurations;

public sealed class QuotationApprovalRequestConfiguration : IEntityTypeConfiguration<QuotationApprovalRequest>
{
    public void Configure(EntityTypeBuilder<QuotationApprovalRequest> builder)
    {
        builder.ToTable("quotation_approval_requests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.QuotationId).HasColumnName("quotation_id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.RevisionId).HasColumnName("revision_id");
        builder.Property(x => x.Reason).HasColumnName("reason");
        builder.Property(x => x.TotalAmount).HasColumnName("total_amount");
        builder.Property(x => x.MarginPercent).HasColumnName("margin_percent");
        builder.Property(x => x.DiscountPercent).HasColumnName("discount_percent");
        builder.Property(x => x.Status).HasConversion<string>().HasColumnName("status");
        builder.Property(x => x.RequestedByUserId).HasColumnName("requested_by_user_id");
        builder.Property(x => x.ReviewedByUserId).HasColumnName("reviewed_by_user_id");
        builder.Property(x => x.DecisionReason).HasColumnName("decision_reason");
        builder.Property(x => x.RequestedAt).HasColumnName("requested_at");
        builder.Property(x => x.ReviewedAt).HasColumnName("reviewed_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.HasIndex(x => new { x.QuotationId, x.Status, x.RequestedAt });
    }
}
