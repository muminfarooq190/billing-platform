using MediatR;

namespace TravelService.Application.Commands.DeleteQuotationAttachment;

public sealed record DeleteQuotationAttachmentCommand(Guid TenantId, Guid QuotationId, Guid AttachmentId) : IRequest;
