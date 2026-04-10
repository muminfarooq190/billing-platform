using FluentAssertions;
using IdentityService.Domain.Aggregates;
using IdentityService.Domain.Exceptions;

namespace IdentityService.Tests.Domain;

public sealed class UserInvitationTests
{
    [Fact]
    public void Accept_ShouldMarkInvitationAccepted()
    {
        var invitation = UserInvitation.Create(Guid.NewGuid(), "invitee@example.com", "Member", Guid.NewGuid(), "tokenhash", DateTimeOffset.UtcNow.AddHours(24));

        invitation.Accept();

        invitation.AcceptedAt.Should().NotBeNull();
        invitation.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Resend_ShouldReplaceTokenAndExpiry()
    {
        var invitation = UserInvitation.Create(Guid.NewGuid(), "invitee@example.com", "Member", Guid.NewGuid(), "tokenhash", DateTimeOffset.UtcNow.AddHours(24));
        var later = DateTimeOffset.UtcNow.AddHours(48);

        invitation.Resend("newhash", later);

        invitation.TokenHash.Should().Be("newhash");
        invitation.ExpiresAt.Should().Be(later);
    }

    [Fact]
    public void Accept_ShouldFailForExpiredInvitation()
    {
        var invitation = UserInvitation.Create(Guid.NewGuid(), "invitee@example.com", "Member", Guid.NewGuid(), "tokenhash", DateTimeOffset.UtcNow.AddMinutes(1));
        invitation.Resend("tokenhash", DateTimeOffset.UtcNow.AddSeconds(-1));

        var act = () => invitation.Accept();

        act.Should().Throw<DomainException>().WithMessage("*expired*");
    }
}
