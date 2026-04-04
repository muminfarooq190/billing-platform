using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace CommunicationService.Infrastructure.Channels;

public sealed class EmailChannelOptions
{
    public const string SectionName = "Communication:Email";

    public string Provider { get; set; } = "log";
    public string? DefaultFromEmail { get; set; }
    public string? DefaultFromName { get; set; }
    public string? SendGridApiKey { get; set; }
    public string SendGridBaseUrl { get; set; } = "https://api.sendgrid.com/";
}

public sealed class SmsChannelOptions
{
    public const string SectionName = "Communication:Sms";

    public string Provider { get; set; } = "log";
    public string? DefaultFromNumber { get; set; }
    public string? TwilioAccountSid { get; set; }
    public string? TwilioAuthToken { get; set; }
    public string TwilioBaseUrl { get; set; } = "https://api.twilio.com/";
}

public sealed class EmailChannelOptionsValidator : IValidateOptions<EmailChannelOptions>
{
    public ValidateOptionsResult Validate(string? name, EmailChannelOptions options)
    {
        var provider = options.Provider.Trim();
        if (provider.Equals("sendgrid", StringComparison.OrdinalIgnoreCase))
        {
            var failures = new List<string>();
            if (string.IsNullOrWhiteSpace(options.SendGridApiKey)) failures.Add("Communication:Email:SendGridApiKey is required when EMAIL_PROVIDER=sendgrid.");
            if (string.IsNullOrWhiteSpace(options.DefaultFromEmail)) failures.Add("Communication:Email:DefaultFromEmail is required when EMAIL_PROVIDER=sendgrid.");
            if (!Uri.TryCreate(options.SendGridBaseUrl, UriKind.Absolute, out _)) failures.Add("Communication:Email:SendGridBaseUrl must be an absolute URI.");

            return failures.Count > 0 ? ValidateOptionsResult.Fail(failures) : ValidateOptionsResult.Success;
        }

        return ValidateOptionsResult.Success;
    }
}

public sealed class SmsChannelOptionsValidator : IValidateOptions<SmsChannelOptions>
{
    public ValidateOptionsResult Validate(string? name, SmsChannelOptions options)
    {
        var provider = options.Provider.Trim();
        if (provider.Equals("twilio", StringComparison.OrdinalIgnoreCase))
        {
            var failures = new List<string>();
            if (string.IsNullOrWhiteSpace(options.TwilioAccountSid)) failures.Add("Communication:Sms:TwilioAccountSid is required when SMS_PROVIDER=twilio.");
            if (string.IsNullOrWhiteSpace(options.TwilioAuthToken)) failures.Add("Communication:Sms:TwilioAuthToken is required when SMS_PROVIDER=twilio.");
            if (string.IsNullOrWhiteSpace(options.DefaultFromNumber)) failures.Add("Communication:Sms:DefaultFromNumber is required when SMS_PROVIDER=twilio.");
            if (!Uri.TryCreate(options.TwilioBaseUrl, UriKind.Absolute, out _)) failures.Add("Communication:Sms:TwilioBaseUrl must be an absolute URI.");

            return failures.Count > 0 ? ValidateOptionsResult.Fail(failures) : ValidateOptionsResult.Success;
        }

        return ValidateOptionsResult.Success;
    }
}
