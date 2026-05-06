using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelService.Domain.Aggregates;

namespace TravelService.Infrastructure.Persistence.Configurations;

public sealed class TravelTemplateConfiguration : IEntityTypeConfiguration<TravelTemplate>
{
    public void Configure(EntityTypeBuilder<TravelTemplate> builder)
    {
        builder.ToTable("travel_templates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.Context).HasColumnName("context").HasConversion<string>().IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasColumnType("text");
        builder.Property(x => x.Category).HasColumnName("category").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Banner).HasColumnName("banner").HasMaxLength(500).IsRequired();
        builder.Property(x => x.AccentColor).HasColumnName("accent_color").HasMaxLength(16).IsRequired();
        builder.Property(x => x.Tagline).HasColumnName("tagline").HasMaxLength(200).IsRequired();
        builder.Property(x => x.SectionsJson).HasColumnName("sections_json").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.SeedJson).HasColumnName("seed_json").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.IsBuiltIn).HasColumnName("is_built_in").IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => new { x.TenantId, x.Context, x.IsActive });
    }
}
