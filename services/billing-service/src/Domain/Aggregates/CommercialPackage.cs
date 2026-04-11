using BillingService.Domain.Common;
using BillingService.Domain.Exceptions;

namespace BillingService.Domain.Aggregates;

public sealed class CommercialPackage : AggregateRoot
{
    private CommercialPackage() { }

    private CommercialPackage(string code, string name, string category, string billingModel, string description, bool isActive, string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Code is required.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required.");
        if (string.IsNullOrWhiteSpace(category))
            throw new DomainException("Category is required.");
        if (string.IsNullOrWhiteSpace(billingModel))
            throw new DomainException("Billing model is required.");
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Description is required.");

        Id = Guid.NewGuid();
        Code = code.Trim();
        Name = name.Trim();
        Category = category.Trim();
        BillingModel = billingModel.Trim();
        Description = description.Trim();
        IsActive = isActive;
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson;
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
        => new(code, name, category, billingModel, description, isActive, metadataJson);

    public void Update(string code, string name, string category, string billingModel, string description, bool isActive, string? metadataJson)
    {
        if (DeletedAt is not null)
            throw new DomainException("Cannot update a deleted commercial package.");

        Code = code.Trim();
        Name = name.Trim();
        Category = category.Trim();
        BillingModel = billingModel.Trim();
        Description = description.Trim();
        IsActive = isActive;
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
