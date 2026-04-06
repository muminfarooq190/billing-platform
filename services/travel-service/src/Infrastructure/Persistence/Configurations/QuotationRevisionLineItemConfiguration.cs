using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelService.Domain.Aggregates;

namespace TravelService.Infrastructure.Persistence.Configurations;

public sealed class QuotationRevisionLineItemConfiguration : IEntityTypeConfiguration<QuotationRevisionLineItem>
{
    public void Configure(EntityTypeBuilder<QuotationRevisionLineItem> builder)
    {
        builder.ToTable("quotation_revision_line_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.QuotationRevisionId).HasColumnName("quotation_revision_id");
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(512);
        builder.Property(x => x.Quantity).HasColumnName("quantity");
        builder.Property(x => x.UnitPriceAmount).HasColumnName("unit_price_amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
        builder.Property(x => x.SortOrder).HasColumnName("sort_order");
        builder.Ignore(x => x.LineTotal);
    }
}
