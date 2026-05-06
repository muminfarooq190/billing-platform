using FluentAssertions;
using IdentityService.Domain.Aggregates;
using IdentityService.Domain.Exceptions;

namespace IdentityService.Tests.Domain;

public sealed class PasswordResetTokenTests
{
    [Fact]
    public void Consume_ShouldMarkTokenConsumed()
    {
        var token = PasswordResetToken.Create(Guid.NewGuid(), "user@example.com", "hash", DateTimeOffset.UtcNow.AddMinutes(30));

        token.Consume();

        token.ConsumedAt.Should().NotBeNull();
    }

    [Fact]
    public void Consume_ShouldThrowWhenAlreadyConsumed()
    {
        var token = PasswordResetToken.Create(Guid.NewGuid(), "user@example.com", "hash", DateTimeOffset.UtcNow.AddMinutes(30));
        token.Consume();

        var act = () => token.Consume();

        act.Should().Throw<DomainException>().WithMessage("*already been consumed*");
    }
}
