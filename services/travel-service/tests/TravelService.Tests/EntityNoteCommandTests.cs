using FluentAssertions;
using TravelService.Application.Abstractions;
using TravelService.Application.Commands.EntityNotes;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Tests;

public sealed class EntityNoteCommandTests
{
    [Fact]
    public async Task CreateNote_ShouldPersistEntityScopedNote()
    {
        var repository = new InMemoryEntityNoteRepository();
        var handler = new CreateEntityNoteCommandHandler(repository, new FakeActorContext(Guid.NewGuid()), new RecordingActivityWriter(), new NoOpUnitOfWork());
        var tenantId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        var noteId = await handler.Handle(new CreateEntityNoteCommand(tenantId, "Quotation", entityId, "Internal", "Call customer tomorrow"), CancellationToken.None);

        var note = await repository.GetByIdAsync(noteId, CancellationToken.None);
        note.Should().NotBeNull();
        note!.TenantId.Should().Be(tenantId);
        note.EntityType.Should().Be("Quotation");
        note.EntityId.Should().Be(entityId);
    }

    [Fact]
    public async Task UpdateNote_ShouldChangeVisibilityAndContent()
    {
        var tenantId = Guid.NewGuid();
        var note = EntityNote.Create(tenantId, "Booking", Guid.NewGuid(), "Internal", "Old note", Guid.NewGuid());
        var repository = new InMemoryEntityNoteRepository(note);
        var handler = new UpdateEntityNoteCommandHandler(repository, new NoOpUnitOfWork());

        await handler.Handle(new UpdateEntityNoteCommand(tenantId, note.Id, "CustomerVisible", "Updated note"), CancellationToken.None);

        note.Visibility.Should().Be("CustomerVisible");
        note.Content.Should().Be("Updated note");
    }

    [Fact]
    public async Task DeleteNote_ShouldSoftDeleteAndHideFromList()
    {
        var tenantId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var note = EntityNote.Create(tenantId, "Traveler", entityId, "Internal", "Passport pending", Guid.NewGuid());
        var repository = new InMemoryEntityNoteRepository(note);
        var handler = new DeleteEntityNoteCommandHandler(repository, new NoOpUnitOfWork());

        await handler.Handle(new DeleteEntityNoteCommand(tenantId, note.Id), CancellationToken.None);

        note.DeletedAt.Should().NotBeNull();
        var items = await repository.ListByEntityAsync(tenantId, "Traveler", entityId, CancellationToken.None);
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateNote_ShouldRejectCrossTenantMutation()
    {
        var note = EntityNote.Create(Guid.NewGuid(), "Booking", Guid.NewGuid(), "Internal", "Private", Guid.NewGuid());
        var repository = new InMemoryEntityNoteRepository(note);
        var handler = new UpdateEntityNoteCommandHandler(repository, new NoOpUnitOfWork());

        var act = async () => await handler.Handle(new UpdateEntityNoteCommand(Guid.NewGuid(), note.Id, "Internal", "Hacked"), CancellationToken.None);

        await act.Should().ThrowAsync<TravelService.Domain.Exceptions.DomainException>().WithMessage("*active tenant*");
    }

    private sealed class InMemoryEntityNoteRepository(params EntityNote[] notes) : IEntityNoteRepository
    {
        private readonly List<EntityNote> _notes = notes.ToList();

        public Task AddAsync(EntityNote note, CancellationToken cancellationToken)
        {
            _notes.Add(note);
            return Task.CompletedTask;
        }

        public Task<EntityNote?> GetByIdAsync(Guid noteId, CancellationToken cancellationToken)
            => Task.FromResult(_notes.SingleOrDefault(x => x.Id == noteId));

        public Task<IReadOnlyList<EntityNote>> ListByEntityAsync(Guid tenantId, string entityType, Guid entityId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<EntityNote>>(_notes.Where(x => x.TenantId == tenantId && x.EntityType == entityType && x.EntityId == entityId && x.DeletedAt is null).ToList());

        public Task UpdateAsync(EntityNote note, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class RecordingActivityWriter : IActivityWriter
    {
        public List<ActivityEntry> Entries { get; } = [];
        public Task WriteAsync(ActivityEntry entry, CancellationToken cancellationToken) { Entries.Add(entry); return Task.CompletedTask; }
    }

    private sealed class FakeActorContext(Guid tenantId) : IActorContext
    {
        public Guid? UserId { get; } = Guid.NewGuid();
        public Guid TenantId { get; } = tenantId;
        public string? IpAddress { get; } = "127.0.0.1";
        public string? UserAgent { get; } = "tests";
    }

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(1);
    }
}
