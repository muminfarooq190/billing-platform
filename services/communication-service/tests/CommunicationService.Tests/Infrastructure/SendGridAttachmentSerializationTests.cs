using CommunicationService.Infrastructure.Channels;
using FluentAssertions;

namespace CommunicationService.Tests.Infrastructure;

public sealed class SendGridAttachmentSerializationTests
{
    [Fact]
    public void EmailAttachmentReference_ShouldCarryBinaryContent()
    {
        var bytes = new byte[] { 1, 2, 3, 4 };
        var attachment = new EmailAttachmentReference("quote.pdf", "https://example.com/quote.pdf", "application/pdf", bytes);

        attachment.Content.Should().NotBeNull();
        attachment.Content.Should().Equal(bytes);
    }
}
