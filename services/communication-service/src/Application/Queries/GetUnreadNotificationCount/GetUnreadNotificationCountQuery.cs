using MediatR;

namespace CommunicationService.Application.Queries.GetUnreadNotificationCount;

public sealed record GetUnreadNotificationCountQuery(Guid RecipientId) : IRequest<int>;
