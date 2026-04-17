using MediatR;

namespace TravelService.Application.Commands.TravelInquiries;

public sealed record QualifyInquiryCommand(Guid TenantId, Guid InquiryId, string? Reason) : IRequest;
