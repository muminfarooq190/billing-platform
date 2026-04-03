using BillingService.Domain.Aggregates;
using BillingService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace BillingService.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");
        builder.HasKey(x => x.Id);
        builder.OwnsOne(x => x.Subtotal, money =>
        {
            money.Property(m => m.Amount).HasColumnName("subtotal_amount");
            money.Property(m => m.Currency).HasColumnName("subtotal_currency");
        });
        builder.OwnsOne(x => x.TaxAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("tax_amount");
            money.Property(m => m.Currency).HasColumnName("tax_currency");
        });
        builder.OwnsOne(x => x.Total, money =>
        {
            money.Property(m => m.Amount).HasColumnName("total_amount");
            money.Property(m => m.Currency).HasColumnName("total_currency");
        });
        builder.Property(x => x.Status).HasConversion<string>();
        builder.Property(x => x.DueDate).HasColumnName("due_date");
        builder.Property(x => x.PaidAt).HasColumnName("paid_at");
        builder.Property(x => x.IssuedAt).HasColumnName("issued_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Ignore(x => x.LineItems);
    }
}
