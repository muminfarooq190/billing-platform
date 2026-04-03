using BillingService.Application.Abstractions;
using BillingService.Application.Commands.CreateSubscription;
using BillingService.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace BillingService.Tests.Application;

public sealed class CreateSubscriptionCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnSubscriptionId()
    {
        var repo = new Mock<ISubscriptionRepository>();
        var uow = new Mock<IUnitOfWork>();
        var handler = new CreateSubscriptionCommandHandler(repo.Object, uow.Object);

        var id = await handler.Handle(new CreateSubscriptionCommand(Guid.NewGuid(), "Pro", "Monthly"), CancellationToken.None);

        id.Should().NotBeEmpty();
    }
}
