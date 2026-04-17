using MediatR;

namespace TravelService.Application.Commands.TravelInquiries;

public sealed record MarkInquiryContactedCommand(Guid TenantId, Guid InquiryId, string? Reason) : IRequest;
