using BillingService.Domain.Aggregates;
using BillingService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace BillingService.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        var moneyConverter = new ValueConverter<Money, string>(
            money => JsonSerializer.Serialize(money, (JsonSerializerOptions?)null),
            json => JsonSerializer.Deserialize<Money>(json, (JsonSerializerOptions?)null));

        builder.ToTable("invoices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.SubscriptionId).HasColumnName("SubscriptionId");
        builder.Property(x => x.TenantId).HasColumnName("TenantId");
        builder.Property(x => x.Subtotal).HasConversion(moneyConverter).HasColumnName("subtotal");
        builder.Property(x => x.TaxAmount).HasConversion(moneyConverter).HasColumnName("tax_amount");
        builder.Property(x => x.Total).HasConversion(moneyConverter).HasColumnName("total");
        builder.Property(x => x.Status).HasConversion<string>().HasColumnName("Status");
        builder.Property(x => x.DueDate).HasColumnName("due_date");
        builder.Property(x => x.PaidAt).HasColumnName("paid_at");
        builder.Property(x => x.IssuedAt).HasColumnName("issued_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Ignore(x => x.LineItems);
    }
}
