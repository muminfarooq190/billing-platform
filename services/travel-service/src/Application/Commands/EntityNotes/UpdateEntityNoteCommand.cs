using MediatR;

namespace TravelService.Application.Commands.EntityNotes;

public sealed record UpdateEntityNoteCommand(Guid TenantId, Guid NoteId, string Visibility, string Content) : IRequest;
