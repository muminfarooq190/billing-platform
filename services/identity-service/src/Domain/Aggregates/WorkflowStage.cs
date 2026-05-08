using IdentityService.Domain.Common;

namespace IdentityService.Domain.Aggregates;

/// <summary>
/// Per-tenant pipeline stage definition (e.g., Inquiry, Concept, Quote, Booking, Itinerary, Documents).
/// Drives the front-end Workflow Settings page and downstream workflow-hub stage filters.
/// </summary>
public sealed class WorkflowStage : AggregateRoot
{
    private WorkflowStage() { }

    private WorkflowStage(
        Guid id,
        Guid tenantId,
        string key,
        string label,
        string color,
        string icon,
        int sortOrder,
        bool required,
        string templateContext,
        string automationType,
        string automationPayloadJson)
    {
        Id = id;
        TenantId = tenantId;
        Key = key;
        Label = label;
        Color = color;
        Icon = icon;
        SortOrder = sortOrder;
        Required = required;
        TemplateContext = templateContext;
        AutomationType = automationType;
        AutomationPayloadJson = automationPayloadJson;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public string Color { get; private set; } = "#041627";
    public string Icon { get; private set; } = "view_kanban";
    public int SortOrder { get; private set; }
    public bool Required { get; private set; }
    public string TemplateContext { get; private set; } = string.Empty;
    public string AutomationType { get; private set; } = "none";
    public string AutomationPayloadJson { get; private set; } = "{}";
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static WorkflowStage Create(
        Guid tenantId,
        string key,
        string label,
        string color,
        string icon,
        int sortOrder,
        bool required,
        string templateContext,
        string automationType,
        string automationPayloadJson)
        => new(
            Guid.NewGuid(),
            tenantId,
            NormalizeKey(key),
            NormalizeOrDefault(label, "Stage"),
            NormalizeOrDefault(color, "#041627"),
            NormalizeOrDefault(icon, "view_kanban"),
            Math.Max(0, sortOrder),
            required,
            templateContext?.Trim() ?? string.Empty,
            NormalizeOrDefault(automationType, "none"),
            string.IsNullOrWhiteSpace(automationPayloadJson) ? "{}" : automationPayloadJson);

    public void Update(
        string label,
        string color,
        string icon,
        int sortOrder,
        bool required,
        string templateContext,
        string automationType,
        string automationPayloadJson)
    {
        Label = NormalizeOrDefault(label, Label);
        Color = NormalizeOrDefault(color, Color);
        Icon = NormalizeOrDefault(icon, Icon);
        SortOrder = Math.Max(0, sortOrder);
        Required = required;
        TemplateContext = templateContext?.Trim() ?? string.Empty;
        AutomationType = NormalizeOrDefault(automationType, "none");
        AutomationPayloadJson = string.IsNullOrWhiteSpace(automationPayloadJson) ? "{}" : automationPayloadJson;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string NormalizeKey(string? value)
    {
        var normalized = value?.Trim().ToLowerInvariant().Replace(' ', '-');
        if (string.IsNullOrWhiteSpace(normalized))
            throw new InvalidOperationException("Workflow stage key is required.");
        return normalized;
    }

    private static string NormalizeOrDefault(string? value, string fallback)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized)) return fallback;
        return normalized;
    }
}
