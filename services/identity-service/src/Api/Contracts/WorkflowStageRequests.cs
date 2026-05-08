namespace IdentityService.Api.Contracts;

public sealed record CreateWorkflowStageRequest(
    string Key,
    string Label,
    string? Color,
    string? Icon,
    int? SortOrder,
    bool? Required,
    string? TemplateContext,
    string? AutomationType,
    string? AutomationPayloadJson);

public sealed record UpdateWorkflowStageRequest(
    string Label,
    string? Color,
    string? Icon,
    int? SortOrder,
    bool? Required,
    string? TemplateContext,
    string? AutomationType,
    string? AutomationPayloadJson);

public sealed record WorkflowStageResponse(
    Guid Id,
    Guid TenantId,
    string Key,
    string Label,
    string Color,
    string Icon,
    int SortOrder,
    bool Required,
    string TemplateContext,
    string AutomationType,
    string AutomationPayloadJson,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
