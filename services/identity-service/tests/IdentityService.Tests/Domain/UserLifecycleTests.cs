using FluentAssertions;
using IdentityService.Domain.Aggregates;
using IdentityService.Domain.Enums;
using IdentityService.Domain.Exceptions;
using IdentityService.Domain.ValueObjects;

namespace IdentityService.Tests.Domain;

public sealed class UserLifecycleTests
{
    [Fact]
    public void Invite_ShouldCreateInvitedUserRequiringPasswordSetup()
    {
        var user = User.Invite(new TenantId(Guid.NewGuid()), new Email("invitee@example.com"), UserRole.Member);

        user.Status.Should().Be(UserStatus.Invited);
        user.MustChangePassword.Should().BeTrue();
        user.PasswordHash.Should().BeEmpty();
    }

    [Fact]
    public void AcceptInvitation_ShouldActivateUser()
    {
        var user = User.Invite(new TenantId(Guid.NewGuid()), new Email("invitee@example.com"), UserRole.Member);

        user.AcceptInvitation("hashed-password");

        user.Status.Should().Be(UserStatus.Active);
        user.MustChangePassword.Should().BeFalse();
        user.PasswordHash.Should().Be("hashed-password");
        user.PasswordChangedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkLogin_ShouldThrowForInvitedUser()
    {
        var user = User.Invite(new TenantId(Guid.NewGuid()), new Email("invitee@example.com"), UserRole.Member);

        var act = () => user.MarkLogin();

        act.Should().Throw<DomainException>().WithMessage("*accept invitation*");
    }

    [Fact]
    public void SuspendAndReactivate_ShouldChangeStatus()
    {
        var user = User.Create(new TenantId(Guid.NewGuid()), new Email("active@example.com"), "hash", UserRole.Admin);

        user.Suspend();
        user.Status.Should().Be(UserStatus.Suspended);

        user.Reactivate();
        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void RequestPasswordReset_ShouldCreateTokenAndDomainEvent()
    {
        var user = User.Create(new TenantId(Guid.NewGuid()), new Email("active@example.com"), "hash", UserRole.Admin);

        var token = user.RequestPasswordReset(TimeSpan.FromMinutes(15));

        token.UserId.Should().Be(user.Id);
        token.Email.Should().Be(user.Email);
        token.TokenHash.Should().NotBeNullOrWhiteSpace();
        user.DomainEvents.Should().ContainSingle(x => x is IdentityService.Domain.Events.UserPasswordResetRequestedEvent);
    }
}
