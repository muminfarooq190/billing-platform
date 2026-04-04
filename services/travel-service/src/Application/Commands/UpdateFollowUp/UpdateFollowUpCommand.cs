using MediatR;

namespace TravelService.Application.Commands.UpdateFollowUp;

public sealed record UpdateFollowUpCommand(
    Guid Id,
    string Subject,
    string Notes,
    string Priority,
    DateTimeOffset DueDate,
    Guid? AssignedToUserId,
    string Status) : IRequest;
