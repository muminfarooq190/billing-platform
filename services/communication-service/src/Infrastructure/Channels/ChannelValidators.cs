using System.Net.Mail;
using System.Text.RegularExpressions;

namespace CommunicationService.Infrastructure.Channels;

internal static partial class ChannelValidators
{
    private const string UnknownRecipient = "unknown";

    public static bool IsKnownEmail(string recipient)
    {
        if (string.IsNullOrWhiteSpace(recipient) || recipient.Equals(UnknownRecipient, StringComparison.OrdinalIgnoreCase))
            return false;

        try
        {
            _ = new MailAddress(recipient);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsKnownPhoneNumber(string recipient)
        => !string.IsNullOrWhiteSpace(recipient)
           && !recipient.Equals(UnknownRecipient, StringComparison.OrdinalIgnoreCase)
           && E164PhoneRegex().IsMatch(recipient);

    [GeneratedRegex("^\\+[1-9]\\d{6,14}$", RegexOptions.Compiled)]
    private static partial Regex E164PhoneRegex();
}
