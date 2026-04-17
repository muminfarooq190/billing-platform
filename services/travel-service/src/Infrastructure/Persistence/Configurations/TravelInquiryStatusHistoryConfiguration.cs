using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelService.Domain.Aggregates;

namespace TravelService.Infrastructure.Persistence.Configurations;

public sealed class TravelInquiryStatusHistoryConfiguration : IEntityTypeConfiguration<TravelInquiryStatusHistory>
{
    public void Configure(EntityTypeBuilder<TravelInquiryStatusHistory> builder)
    {
        builder.ToTable("travel_inquiry_status_history");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TravelInquiryId).HasColumnName("travel_inquiry_id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.FromStatus).HasColumnName("from_status").HasMaxLength(64);
        builder.Property(x => x.ToStatus).HasColumnName("to_status").HasMaxLength(64);
        builder.Property(x => x.Reason).HasColumnName("reason");
        builder.Property(x => x.ChangedByUserId).HasColumnName("changed_by_user_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(x => new { x.TenantId, x.TravelInquiryId, x.CreatedAt });

        builder.HasOne<TravelInquiry>()
            .WithMany()
            .HasForeignKey(x => x.TravelInquiryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
