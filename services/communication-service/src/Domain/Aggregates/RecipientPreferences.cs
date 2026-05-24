using System.Text.Json;
using CommunicationService.Domain.Common;
using CommunicationService.Domain.Enums;
using CommunicationService.Domain.Events;
using CommunicationService.Domain.Exceptions;
using CommunicationService.Domain.ValueObjects;

namespace CommunicationService.Domain.Aggregates;

public sealed class RecipientPreferences : AggregateRoot
{
    private readonly List<ChannelPreference> _channelPreferences = [];
    private RecipientPreferences() { }

    private RecipientPreferences(Guid tenantId, Guid recipientId, RecipientType recipientType, string email, string? phone, string? deviceToken)
    {
        if (tenantId == Guid.Empty) throw new DomainException("Tenant id is required.");
        if (recipientId == Guid.Empty) throw new DomainException("Recipient id is required.");
        if (string.IsNullOrWhiteSpace(email)) throw new DomainException("Email is required.");

        Id = Guid.NewGuid();
        TenantId = tenantId;
        RecipientId = recipientId;
        RecipientType = recipientType;
        Email = email.Trim();
        Phone = phone?.Trim() ?? string.Empty;
        DeviceToken = deviceToken?.Trim() ?? string.Empty;
        Timezone = "UTC";
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;

        _channelPreferences.Add(new ChannelPreference(ChannelType.Email, true, false, null, null));
        _channelPreferences.Add(new ChannelPreference(ChannelType.InApp, true, false, null, null));
        SyncChannelPreferencesJson();
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid RecipientId { get; private set; }
    public RecipientType RecipientType { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string DeviceToken { get; private set; } = string.Empty;
    public string Timezone { get; private set; } = "UTC";
    public IReadOnlyList<ChannelPreference> ChannelPreferences => _channelPreferences.AsReadOnly();
    public string ChannelPreferencesJson { get; private set; } = "[]";
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static RecipientPreferences Create(Guid tenantId, Guid recipientId, RecipientType recipientType, string email, string? phone, string? deviceToken)
        => new(tenantId, recipientId, recipientType, email, phone, deviceToken);

    public void UpdateContactInfo(string email, string? phone, string? deviceToken)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new DomainException("Email is required.");

        Email = email.Trim();
        Phone = phone?.Trim() ?? string.Empty;
        DeviceToken = deviceToken?.Trim() ?? string.Empty;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new PreferencesUpdatedEvent(Id, TenantId, RecipientId));
    }

    public void SetChannelPreferences(List<ChannelPreference> preferences)
    {
        if (preferences.Count == 0)
            throw new DomainException("At least one channel preference is required.");

        var distinctChannels = preferences.Select(x => x.Channel).Distinct().Count();
        if (distinctChannels != preferences.Count)
            throw new DomainException("Channel preferences must not contain duplicate channels.");

        if (preferences.Any(x => x.QuietHoursEnabled && (x.QuietStart is null || x.QuietEnd is null)))
            throw new DomainException("Quiet hours require both start and end values.");

        _channelPreferences.Clear();
        _channelPreferences.AddRange(preferences);
        SyncChannelPreferencesJson();
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new PreferencesUpdatedEvent(Id, TenantId, RecipientId));
    }

    public void SetTimezone(string timezone)
    {
        if (string.IsNullOrWhiteSpace(timezone))
            throw new DomainException("Timezone is required.");

        Timezone = timezone.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsChannelEnabled(ChannelType channel)
        => _channelPreferences.Any(p => p.Channel == channel && p.Enabled);

    public ChannelPreference? GetPreference(ChannelType channel)
        => _channelPreferences.FirstOrDefault(p => p.Channel == channel);

    /// <summary>
    /// Honors a STOP / UNSUBSCRIBE / opt-out request for the given channel.
    /// Sets the existing preference row's Enabled=false (or inserts a
    /// disabled row when one didn't exist). Returns true when state
    /// actually changed.
    ///
    /// TCPA (US) + GDPR (EU) require honouring opt-outs within a short
    /// window — this is the canonical aggregate transition the Twilio
    /// inbound consumer calls when it sees STOP.
    /// </summary>
    public bool OptOutChannel(ChannelType channel)
    {
        var existing = _channelPreferences.FirstOrDefault(p => p.Channel == channel);
        if (existing is null)
        {
            _channelPreferences.Add(new ChannelPreference(channel, false, false, null, null));
            SyncChannelPreferencesJson();
            UpdatedAt = DateTimeOffset.UtcNow;
            AddDomainEvent(new PreferencesUpdatedEvent(Id, TenantId, RecipientId));
            return true;
        }
        if (!existing.Enabled) return false;
        _channelPreferences.Remove(existing);
        _channelPreferences.Add(existing with { Enabled = false });
        SyncChannelPreferencesJson();
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new PreferencesUpdatedEvent(Id, TenantId, RecipientId));
        return true;
    }

    public void LoadChannelPreferencesFromJson()
    {
        _channelPreferences.Clear();

        var parsed = string.IsNullOrWhiteSpace(ChannelPreferencesJson)
            ? []
            : JsonSerializer.Deserialize<List<ChannelPreference>>(ChannelPreferencesJson) ?? [];

        if (parsed.Count == 0)
        {
            parsed =
            [
                new ChannelPreference(ChannelType.Email, true, false, null, null),
                new ChannelPreference(ChannelType.InApp, true, false, null, null)
            ];
        }

        _channelPreferences.AddRange(parsed);
        SyncChannelPreferencesJson();
    }

    private void SyncChannelPreferencesJson() => ChannelPreferencesJson = JsonSerializer.Serialize(_channelPreferences);
}
