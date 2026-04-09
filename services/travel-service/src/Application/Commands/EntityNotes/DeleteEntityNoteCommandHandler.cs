using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.EntityNotes;

public sealed class DeleteEntityNoteCommandHandler(
    IEntityNoteRepository entityNoteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteEntityNoteCommand>
{
    public async Task Handle(DeleteEntityNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await entityNoteRepository.GetByIdAsync(request.NoteId, cancellationToken)
            ?? throw new DomainException($"Note {request.NoteId} not found.");

        if (note.TenantId != request.TenantId)
            throw new DomainException("Note does not belong to the active tenant.");

        note.Delete();
        await entityNoteRepository.UpdateAsync(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
