using Microsoft.EntityFrameworkCore;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Infrastructure.Persistence.Repositories;

public sealed class DraftTripConceptRepository(TravelDbContext dbContext) : IDraftTripConceptRepository
{
    public Task AddAsync(DraftTripConcept concept, CancellationToken cancellationToken)
        => dbContext.DraftTripConcepts.AddAsync(concept, cancellationToken).AsTask();

    public Task<DraftTripConcept?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.DraftTripConcepts.SingleOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

    public async Task<IReadOnlyList<DraftTripConcept>> ListByInquiryIdAsync(Guid inquiryId, CancellationToken cancellationToken)
        => await dbContext.DraftTripConcepts
            .Where(x => x.TravelInquiryId == inquiryId && x.DeletedAt == null)
            .OrderByDescending(x => x.IsPrimary)
            .ThenByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

    public Task UpdateAsync(DraftTripConcept concept, CancellationToken cancellationToken)
    {
        dbContext.DraftTripConcepts.Update(concept);
        return Task.CompletedTask;
    }
}
