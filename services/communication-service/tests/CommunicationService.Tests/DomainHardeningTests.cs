using CommunicationService.Domain.Aggregates;
using CommunicationService.Domain.Enums;
using CommunicationService.Domain.Exceptions;
using CommunicationService.Domain.ValueObjects;
using FluentAssertions;

namespace CommunicationService.Tests;

public sealed class DomainHardeningTests
{
    [Fact]
    public void Notification_ResetForRetry_ShouldQueueNotificationAndClearLastFailure()
    {
        var notification = Notification.Create(Guid.NewGuid(), Guid.NewGuid(), RecipientType.EndUser, ChannelType.Email, "Invoice ready", "Your invoice is ready", NotificationPriority.Normal, null, null);
        notification.MarkQueued();
        notification.MarkFailed("smtp timeout");

        notification.ResetForRetry();

        notification.Status.Should().Be(NotificationStatus.Queued);
        notification.LastError.Should().BeNull();
    }

    [Fact]
    public void Notification_MarkDelivered_ShouldRequireSentStatus()
    {
        var notification = Notification.Create(Guid.NewGuid(), Guid.NewGuid(), RecipientType.EndUser, ChannelType.Email, "Invoice ready", "Your invoice is ready", NotificationPriority.Normal, null, null);

        var act = () => notification.MarkDelivered();

        act.Should().Throw<DomainException>().WithMessage("*sent notifications*");
    }

    [Fact]
    public void RecipientPreferences_SetChannelPreferences_ShouldRejectDuplicateChannels()
    {
        var preferences = RecipientPreferences.Create(Guid.NewGuid(), Guid.NewGuid(), RecipientType.EndUser, "customer@example.com", null, null);

        var act = () => preferences.SetChannelPreferences([
            new ChannelPreference(ChannelType.Email, true, false, null, null),
            new ChannelPreference(ChannelType.Email, false, false, null, null)
        ]);

        act.Should().Throw<DomainException>().WithMessage("*duplicate*");
    }

    [Fact]
    public void RecipientPreferences_LoadChannelPreferencesFromJson_ShouldHydrateBackedCollection()
    {
        var preferences = RecipientPreferences.Create(Guid.NewGuid(), Guid.NewGuid(), RecipientType.EndUser, "customer@example.com", null, null);
        preferences.SetChannelPreferences([
            new ChannelPreference(ChannelType.Email, false, false, null, null),
            new ChannelPreference(ChannelType.InApp, true, false, null, null),
            new ChannelPreference(ChannelType.Sms, true, true, new TimeOnly(22, 0), new TimeOnly(6, 0))
        ]);

        var rehydrated = RecipientPreferences.Create(Guid.NewGuid(), Guid.NewGuid(), RecipientType.EndUser, "other@example.com", null, null);
        typeof(RecipientPreferences).GetProperty(nameof(RecipientPreferences.ChannelPreferencesJson))!.SetValue(rehydrated, preferences.ChannelPreferencesJson);

        rehydrated.LoadChannelPreferencesFromJson();

        rehydrated.ChannelPreferences.Should().HaveCount(3);
        rehydrated.IsChannelEnabled(ChannelType.Sms).Should().BeTrue();
    }
}
