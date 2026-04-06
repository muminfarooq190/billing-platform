using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelService.Domain.Aggregates;

namespace TravelService.Infrastructure.Persistence.Configurations;

public sealed class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("contacts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(128);
        builder.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(128);
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(256);
        builder.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(64);
        builder.Property(x => x.Company).HasColumnName("company").HasMaxLength(256);
        builder.Property(x => x.Notes).HasColumnName("notes");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.TagsJson).HasColumnName("tags").HasColumnType("jsonb");
        builder.Ignore(x => x.Tags);
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.Email);
    }
}
