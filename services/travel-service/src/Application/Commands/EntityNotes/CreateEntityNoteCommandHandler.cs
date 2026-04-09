using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.EntityNotes;

public sealed class CreateEntityNoteCommandHandler(
    IEntityNoteRepository entityNoteRepository,
    IActorContext actorContext,
    IActivityWriter activityWriter,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateEntityNoteCommand, Guid>
{
    public async Task<Guid> Handle(CreateEntityNoteCommand request, CancellationToken cancellationToken)
    {
        EnsureSupportedEntityType(request.EntityType);

        var note = EntityNote.Create(request.TenantId, request.EntityType, request.EntityId, request.Visibility, request.Content, actorContext.UserId);

        await entityNoteRepository.AddAsync(note, cancellationToken);
        await activityWriter.WriteAsync(
            ActivityEntry.Create(request.TenantId, request.EntityType, request.EntityId, "CommentAdded", $"{request.EntityType} note added", new { note.Id, note.Visibility }, actorContext.UserId),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return note.Id;
    }

    private static void EnsureSupportedEntityType(string entityType)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new DomainException("Entity type is required.");
    }
}
