using CommunicationService.Domain.Enums;

namespace CommunicationService.Domain.ValueObjects;

public sealed record ChannelPreference(ChannelType Channel, bool Enabled, bool QuietHoursEnabled, TimeOnly? QuietStart, TimeOnly? QuietEnd);
