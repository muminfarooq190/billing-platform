using MediatR;

namespace TravelService.Application.Commands.TravelInquiries;

public sealed record ArchiveInquiryCommand(Guid TenantId, Guid InquiryId, string? Reason) : IRequest;
