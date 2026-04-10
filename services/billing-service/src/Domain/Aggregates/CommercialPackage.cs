using BillingService.Domain.Common;

namespace BillingService.Domain.Aggregates;

public sealed class CommercialPackage : AggregateRoot
{
    private CommercialPackage() { }

    private CommercialPackage(string code, string name, string category, string billingModel, string description, bool isActive, string? metadataJson)
    {
        Id = Guid.NewGuid();
        Code = code;
        Name = name;
        Category = category;
        BillingModel = billingModel;
        Description = description;
        IsActive = isActive;
        MetadataJson = metadataJson;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string BillingModel { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public string? MetadataJson { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static CommercialPackage Create(string code, string name, string category, string billingModel, string description, bool isActive = true, string? metadataJson = null)
        => new(code.Trim(), name.Trim(), category.Trim(), billingModel.Trim(), description.Trim(), isActive, metadataJson);
}
