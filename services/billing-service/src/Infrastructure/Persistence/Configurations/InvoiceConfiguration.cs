using System.Text.Json;
using BillingService.Domain.Aggregates;
using BillingService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BillingService.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        var moneyConverter = new ValueConverter<Money, string>(
            money => JsonSerializer.Serialize(money, (JsonSerializerOptions?)null),
            json => JsonSerializer.Deserialize<Money>(json, (JsonSerializerOptions?)null)!);

        builder.ToTable("invoices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.SubscriptionId).HasColumnName("SubscriptionId");
        builder.Property(x => x.TenantId).HasColumnName("TenantId");
        builder.Property(x => x.InvoiceNumber).HasColumnName("invoice_number").HasMaxLength(40);
        builder.Property(x => x.Subtotal).HasConversion(moneyConverter).HasColumnName("subtotal");
        builder.Property(x => x.TaxAmount).HasConversion(moneyConverter).HasColumnName("tax_amount");
        builder.Property(x => x.Total).HasConversion(moneyConverter).HasColumnName("total");
        builder.Property(x => x.Status).HasConversion<string>().HasColumnName("Status");
        builder.Property(x => x.DueDate).HasColumnName("due_date");
        builder.Property(x => x.BillingPeriodStart).HasColumnName("billing_period_start");
        builder.Property(x => x.BillingPeriodEnd).HasColumnName("billing_period_end");
        builder.Property(x => x.PricingReference).HasColumnName("pricing_reference").HasMaxLength(256);
        builder.Property(x => x.ProviderPaymentId).HasColumnName("provider_payment_id").HasMaxLength(256);
        builder.Property(x => x.PaymentGateway).HasColumnName("payment_gateway").HasMaxLength(64);
        builder.Property(x => x.PaymentFailureCode).HasColumnName("payment_failure_code").HasMaxLength(128);
        builder.Property(x => x.PaymentFailureMessage).HasColumnName("payment_failure_message").HasMaxLength(1024);
        builder.Property(x => x.PaidAt).HasColumnName("paid_at");
        builder.Property(x => x.IssuedAt).HasColumnName("issued_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => new { x.SubscriptionId, x.BillingPeriodStart, x.BillingPeriodEnd }).IsUnique().HasFilter("deleted_at IS NULL");
        builder.Navigation(x => x.LineItems).HasField("_lineItems");

        builder.OwnsMany(x => x.LineItems, lineItemBuilder =>
        {
            lineItemBuilder.ToTable("invoice_line_items");
            lineItemBuilder.WithOwner().HasForeignKey("InvoiceId");
            lineItemBuilder.Property<Guid>("Id");
            lineItemBuilder.HasKey("Id");
            lineItemBuilder.Property<Guid>("InvoiceId").HasColumnName("invoice_id");
            lineItemBuilder.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
            lineItemBuilder.Property(x => x.Quantity).HasColumnName("quantity");
            lineItemBuilder.Property(x => x.UnitPrice).HasConversion(moneyConverter).HasColumnName("unit_price");
        });
    }
}
