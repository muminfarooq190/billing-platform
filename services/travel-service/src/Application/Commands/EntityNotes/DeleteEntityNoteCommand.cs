using MediatR;

namespace TravelService.Application.Commands.EntityNotes;

public sealed record DeleteEntityNoteCommand(Guid TenantId, Guid NoteId) : IRequest;
