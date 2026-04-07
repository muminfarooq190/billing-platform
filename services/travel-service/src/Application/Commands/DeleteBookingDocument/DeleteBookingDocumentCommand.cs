using MediatR;

namespace TravelService.Application.Commands.DeleteBookingDocument;

public sealed record DeleteBookingDocumentCommand(Guid TenantId, Guid BookingId, Guid DocumentId) : IRequest;
