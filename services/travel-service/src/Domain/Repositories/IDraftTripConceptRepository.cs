using TravelService.Domain.Aggregates;

namespace TravelService.Domain.Repositories;

public interface IDraftTripConceptRepository
{
    Task AddAsync(DraftTripConcept concept, CancellationToken cancellationToken);
    Task<DraftTripConcept?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<DraftTripConcept>> ListByInquiryIdAsync(Guid inquiryId, CancellationToken cancellationToken);
    Task UpdateAsync(DraftTripConcept concept, CancellationToken cancellationToken);
}
