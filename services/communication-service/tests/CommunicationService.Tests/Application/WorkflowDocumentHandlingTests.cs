using System.Reflection;
using CommunicationService.Application.Commands.SendWorkflowNotification;
using FluentAssertions;

namespace CommunicationService.Tests.Application;

public sealed class WorkflowDocumentHandlingTests
{
    [Fact]
    public void WorkflowDocumentAppend_ShouldPreserveDocumentMarkerForEmailDispatcher()
    {
        var method = typeof(SendWorkflowNotificationCommandHandler).GetMethod("AppendDocuments", BindingFlags.NonPublic | BindingFlags.Static);
        method.Should().NotBeNull();

        var result = (string)method!.Invoke(null, ["Your quotation is ready.", "[{\"name\":\"quote.pdf\",\"url\":\"https://example.com/quote.pdf\"}]"])!;

        result.Should().Contain("[DocumentReferencesJson]");
        result.Should().Contain("quote.pdf");
    }
}
