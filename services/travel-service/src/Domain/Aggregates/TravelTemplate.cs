using TravelService.Domain.Common;
using TravelService.Domain.Enums;
using TravelService.Domain.Exceptions;

namespace TravelService.Domain.Aggregates;

public sealed class TravelTemplate : AggregateRoot
{
    private TravelTemplate() { }

    private TravelTemplate(
        Guid tenantId,
        TravelTemplateContext context,
        string name,
        string? description,
        string category,
        string banner,
        string accentColor,
        string tagline,
        string sectionsJson,
        string seedJson,
        bool isBuiltIn,
        Guid? createdByUserId)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId is required.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Template name is required.");
        if (string.IsNullOrWhiteSpace(category))
            throw new DomainException("Template category is required.");
        if (string.IsNullOrWhiteSpace(banner))
            throw new DomainException("Template banner is required.");
        if (string.IsNullOrWhiteSpace(accentColor))
            throw new DomainException("Template accent color is required.");
        if (string.IsNullOrWhiteSpace(tagline))
            throw new DomainException("Template tagline is required.");
        if (string.IsNullOrWhiteSpace(sectionsJson))
            throw new DomainException("Template sections JSON is required.");
        if (string.IsNullOrWhiteSpace(seedJson))
            throw new DomainException("Template seed JSON is required.");

        Id = Guid.NewGuid();
        TenantId = tenantId;
        Context = context;
        Name = name.Trim();
        Description = NormalizeOptional(description);
        Category = category.Trim();
        Banner = banner.Trim();
        AccentColor = accentColor.Trim();
        Tagline = tagline.Trim();
        SectionsJson = sectionsJson.Trim();
        SeedJson = seedJson.Trim();
        IsBuiltIn = isBuiltIn;
        IsActive = true;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public TravelTemplateContext Context { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public string Banner { get; private set; } = string.Empty;
    public string AccentColor { get; private set; } = string.Empty;
    public string Tagline { get; private set; } = string.Empty;
    public string SectionsJson { get; private set; } = string.Empty;
    public string SeedJson { get; private set; } = string.Empty;
    public bool IsBuiltIn { get; private set; }
    public bool IsActive { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static TravelTemplate Create(
        Guid tenantId,
        TravelTemplateContext context,
        string name,
        string? description,
        string category,
        string banner,
        string accentColor,
        string tagline,
        string sectionsJson,
        string seedJson,
        bool isBuiltIn,
        Guid? createdByUserId)
        => new(tenantId, context, name, description, category, banner, accentColor, tagline, sectionsJson, seedJson, isBuiltIn, createdByUserId);

    public void Update(
        string name,
        string? description,
        string category,
        string banner,
        string accentColor,
        string tagline,
        string sectionsJson,
        string seedJson)
    {
        if (IsBuiltIn)
            throw new DomainException("Built-in templates cannot be edited.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Template name is required.");
        if (string.IsNullOrWhiteSpace(category))
            throw new DomainException("Template category is required.");
        if (string.IsNullOrWhiteSpace(banner))
            throw new DomainException("Template banner is required.");
        if (string.IsNullOrWhiteSpace(accentColor))
            throw new DomainException("Template accent color is required.");
        if (string.IsNullOrWhiteSpace(tagline))
            throw new DomainException("Template tagline is required.");
        if (string.IsNullOrWhiteSpace(sectionsJson))
            throw new DomainException("Template sections JSON is required.");
        if (string.IsNullOrWhiteSpace(seedJson))
            throw new DomainException("Template seed JSON is required.");

        Name = name.Trim();
        Description = NormalizeOptional(description);
        Category = category.Trim();
        Banner = banner.Trim();
        AccentColor = accentColor.Trim();
        Tagline = tagline.Trim();
        SectionsJson = sectionsJson.Trim();
        SeedJson = seedJson.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkActive()
    {
        if (!IsActive)
        {
            IsActive = true;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void Archive()
    {
        if (IsBuiltIn)
            throw new DomainException("Built-in templates cannot be deleted.");

        DeletedAt = DateTimeOffset.UtcNow;
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
