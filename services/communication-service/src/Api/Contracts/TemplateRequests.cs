namespace CommunicationService.Api.Contracts;

public sealed record CreateTemplateRequest(
    string Name,
    string Subject,
    string BodyTemplate,
    string Channel,
    string? Description);

public sealed record UpdateTemplateRequest(
    string Name,
    string Subject,
    string BodyTemplate,
    string Description,
    string? Action);
