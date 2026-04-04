using MediatR;

namespace TravelService.Application.Commands.CreateFollowUp;

public sealed record CreateFollowUpCommand(
    Guid TenantId,
    Guid CustomerContactId,
    string CustomerName,
    string Subject,
    string Notes,
    string Priority,
    DateTimeOffset DueDate,
    Guid? AssignedToUserId) : IRequest<Guid>;
