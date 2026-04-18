using TravelService.Application.Queries.GetItineraryById;
using TravelService.Application.Queries.QuotationRevisions;

namespace TravelService.Api.Documents;

public interface IPdfDocumentRenderer
{
    byte[] RenderQuotationRevisionPdf(QuotationRevisionReadModel revision);
    byte[] RenderItineraryPdf(ItineraryReadModel itinerary);
}
