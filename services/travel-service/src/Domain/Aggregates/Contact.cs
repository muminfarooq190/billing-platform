using System.Text.Json;
using TravelService.Domain.Common;
using TravelService.Domain.Exceptions;

namespace TravelService.Domain.Aggregates;

public sealed class Contact : AggregateRoot
{
    private readonly List<string> _tags = [];

    private Contact() { }

    private Contact(Guid tenantId, string firstName, string lastName, string? email, string? phone, string? company, string? notes, IReadOnlyCollection<string>? tags)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId is required.");

        Id = Guid.NewGuid();
        TenantId = tenantId;
        ApplyDetails(firstName, lastName, email, phone, company, notes, tags);
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Company { get; private set; } = string.Empty;
    public string Notes { get; private set; } = string.Empty;
    public string TagsJson { get; private set; } = "[]";
    public IReadOnlyList<string> Tags => _tags.AsReadOnly();
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static Contact Create(Guid tenantId, string firstName, string lastName, string? email, string? phone, string? company, string? notes, IReadOnlyCollection<string>? tags)
        => new(tenantId, firstName, lastName, email, phone, company, notes, tags);

    public void Update(string firstName, string lastName, string? email, string? phone, string? company, string? notes, IReadOnlyCollection<string>? tags)
    {
        ApplyDetails(firstName, lastName, email, phone, company, notes, tags);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void ApplyDetails(string firstName, string lastName, string? email, string? phone, string? company, string? notes, IReadOnlyCollection<string>? tags)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email?.Trim() ?? string.Empty;
        Phone = phone?.Trim() ?? string.Empty;
        Company = company?.Trim() ?? string.Empty;
        Notes = notes?.Trim() ?? string.Empty;

        _tags.Clear();
        if (tags is null)
            return;

        foreach (var tag in tags.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            _tags.Add(tag);
        }

        TagsJson = JsonSerializer.Serialize(_tags);
    }

    public void LoadTagsFromJson()
    {
        _tags.Clear();

        var parsed = string.IsNullOrWhiteSpace(TagsJson)
            ? []
            : JsonSerializer.Deserialize<List<string>>(TagsJson) ?? [];

        foreach (var tag in parsed.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            _tags.Add(tag);
        }

        TagsJson = JsonSerializer.Serialize(_tags);
    }
}
