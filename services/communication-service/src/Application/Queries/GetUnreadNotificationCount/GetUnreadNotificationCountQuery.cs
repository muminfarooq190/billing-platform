using MediatR;

namespace CommunicationService.Application.Queries.GetUnreadNotificationCount;

public sealed record GetUnreadNotificationCountQuery(Guid TenantId, Guid RecipientId) : IRequest<int>;
