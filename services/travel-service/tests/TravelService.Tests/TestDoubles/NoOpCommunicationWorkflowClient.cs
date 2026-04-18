using TravelService.Application.Abstractions;

namespace TravelService.Tests.TestDoubles;

public sealed class NoOpCommunicationWorkflowClient : ICommunicationWorkflowClient
{
    public Task SendQuotationAsync(Guid tenantId, QuotationCommunicationRequest request, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task SendItineraryAsync(Guid tenantId, ItineraryCommunicationRequest request, CancellationToken cancellationToken) => Task.CompletedTask;
}
