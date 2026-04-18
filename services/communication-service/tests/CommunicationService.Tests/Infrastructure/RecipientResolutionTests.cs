using CommunicationService.Domain.Aggregates;
using CommunicationService.Domain.Enums;
using CommunicationService.Infrastructure.Recipients;
using FluentAssertions;

namespace CommunicationService.Tests.Infrastructure;

public sealed class RecipientResolutionTests
{
    [Fact]
    public async Task RecipientAddressResolver_ShouldUsePreferences_WhenPresent()
    {
        var notification = Notification.Create(Guid.NewGuid(), Guid.NewGuid(), RecipientType.EndUser, ChannelType.Email, "Subject", "Body", NotificationPriority.Normal, null, null);
        var preferences = RecipientPreferences.Create(notification.TenantId, notification.RecipientId, RecipientType.EndUser, "customer@example.com", "+917006501588", null);
        var resolver = new TravelContactRecipientAddressResolver(new HttpClient(new FailingHandler()) { BaseAddress = new Uri("http://localhost") });

        var result = await resolver.ResolveAsync(notification, preferences, CancellationToken.None);

        result.Should().Be("customer@example.com");
    }

    private sealed class FailingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => throw new InvalidOperationException("Network should not be hit when preferences are present.");
    }
}
