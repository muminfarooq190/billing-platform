using MediatR;

namespace TravelService.Application.Commands.TravelInquiries;

public sealed record AssignInquiryCommand(Guid TenantId, Guid InquiryId, Guid? AssignedToUserId) : IRequest;
