using FluentAssertions;
using IdentityService.Domain.Aggregates;

namespace IdentityService.Tests.Domain;

public sealed class UserMfaEnrollmentTests
{
    [Fact]
    public void VerifyAndDisable_ShouldToggleEnabledState()
    {
        var enrollment = UserMfaEnrollment.Create(Guid.NewGuid(), Guid.NewGuid(), "JBSWY3DPEHPK3PXP", "[]");

        enrollment.Verify();
        enrollment.IsEnabled.Should().BeTrue();

        enrollment.Disable();
        enrollment.IsEnabled.Should().BeFalse();
    }
}
