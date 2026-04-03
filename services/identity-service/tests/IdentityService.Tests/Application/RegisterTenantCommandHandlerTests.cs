using FluentAssertions;
using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands.RegisterTenant;
using IdentityService.Domain.Aggregates;
using IdentityService.Domain.Repositories;
using Moq;

namespace IdentityService.Tests.Application;

public sealed class RegisterTenantCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateTenantAndOwner()
    {
        var tenantRepository = new Mock<ITenantRepository>();
        var userRepository = new Mock<IUserRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        tenantRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        var handler = new RegisterTenantCommandHandler(tenantRepository.Object, userRepository.Object, unitOfWork.Object);

        var result = await handler.Handle(new RegisterTenantCommand("Acme", "owner@acme.com", "Demo1234!"), CancellationToken.None);

        result.TenantId.Should().NotBeEmpty();
        result.OwnerUserId.Should().NotBeEmpty();
    }
}
