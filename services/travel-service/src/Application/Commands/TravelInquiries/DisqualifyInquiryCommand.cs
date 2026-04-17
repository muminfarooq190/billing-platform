using MediatR;

namespace TravelService.Application.Commands.TravelInquiries;

public sealed record DisqualifyInquiryCommand(Guid TenantId, Guid InquiryId, string Status, string? Reason) : IRequest;
