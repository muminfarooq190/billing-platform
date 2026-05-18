using TravelService.Application.Queries.GetItineraryById;
using TravelService.Application.Queries.QuotationRevisions;

namespace TravelService.Api.Documents;

public interface IPdfDocumentRenderer
{
    byte[] RenderQuotationRevisionPdf(QuotationRevisionReadModel revision, PdfBranding? branding = null);
    byte[] RenderItineraryPdf(ItineraryReadModel itinerary, PdfBranding? branding = null);
}
