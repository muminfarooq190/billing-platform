using MediatR;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;
using TravelService.Application.Abstractions;

namespace TravelService.Application.Commands.EntityNotes;

public sealed class UpdateEntityNoteCommandHandler(
    IEntityNoteRepository entityNoteRepository,
    IFeatureGate featureGate,
    IUnitOfWork unitOfWork, Api.ITenantContext tenantContext) : IRequestHandler<UpdateEntityNoteCommand>
{
    public async Task Handle(UpdateEntityNoteCommand request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync(FeatureKeys.TravelNotesWrite, request.TenantId, tenantContext.UserId, cancellationToken);

        var note = await entityNoteRepository.GetByIdAsync(request.NoteId, cancellationToken)
            ?? throw new DomainException($"Note {request.NoteId} not found.");

        if (note.TenantId != request.TenantId)
            throw new DomainException("Note does not belong to the active tenant.");

        note.Update(request.Visibility, request.Content);
        await entityNoteRepository.UpdateAsync(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
