using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelService.Domain.Aggregates;

namespace TravelService.Infrastructure.Persistence.Configurations;

public sealed class TravelInquiryConfiguration : IEntityTypeConfiguration<TravelInquiry>
{
    public void Configure(EntityTypeBuilder<TravelInquiry> builder)
    {
        builder.ToTable("travel_inquiries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.Source).HasColumnName("source").HasMaxLength(64);
        builder.Property(x => x.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(64);
        builder.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(256);
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(256);
        builder.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(64);
        builder.Property(x => x.WhatsappNumber).HasColumnName("whatsapp_number").HasMaxLength(64);
        builder.Property(x => x.DepartureCity).HasColumnName("departure_city").HasMaxLength(128);
        builder.Property(x => x.Destination).HasColumnName("destination").HasMaxLength(256);
        builder.Property(x => x.TravelDate).HasColumnName("travel_date");
        builder.Property(x => x.ReturnDate).HasColumnName("return_date");
        builder.Property(x => x.IsDateFlexible).HasColumnName("is_date_flexible");
        builder.Property(x => x.Travellers).HasColumnName("travellers");
        builder.Property(x => x.BudgetAmount).HasColumnName("budget_amount").HasColumnType("numeric(18,2)");
        builder.Property(x => x.BudgetCurrency).HasColumnName("budget_currency").HasMaxLength(3);
        builder.Property(x => x.CustomerMessage).HasColumnName("customer_message");
        builder.Property(x => x.AssignedToUserId).HasColumnName("assigned_to_user_id");
        builder.Property(x => x.QualifiedAt).HasColumnName("qualified_at");
        builder.Property(x => x.ContactedAt).HasColumnName("contacted_at");
        builder.Property(x => x.DisqualifiedAt).HasColumnName("disqualified_at");
        builder.Property(x => x.ConvertedAt).HasColumnName("converted_at");
        builder.Property(x => x.ConvertedContactId).HasColumnName("converted_contact_id");
        builder.Property(x => x.ConvertedQuotationId).HasColumnName("converted_quotation_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAt });
        builder.HasIndex(x => new { x.TenantId, x.AssignedToUserId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.Source, x.CreatedAt });
    }
}
