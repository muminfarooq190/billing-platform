using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelService.Domain.Aggregates;

namespace TravelService.Infrastructure.Persistence.Configurations;

public sealed class BookingPaymentConfiguration : IEntityTypeConfiguration<BookingPayment>
{
    public void Configure(EntityTypeBuilder<BookingPayment> builder)
    {
        builder.ToTable("booking_payments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MilestoneLabel).HasMaxLength(200);
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.PaymentMethod).HasMaxLength(64);
        builder.Property(x => x.ProviderReference).HasMaxLength(200);
        builder.Property(x => x.Status).HasConversion<string>().IsRequired();
        builder.Property(x => x.Notes).HasColumnType("text");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => new { x.TenantId, x.BookingId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.DueDate });
    }
}
