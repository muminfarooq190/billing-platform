using System.Text;
using FluentAssertions;
using TravelService.Api.Documents;
using TravelService.Application.Queries.GetItineraryById;
using TravelService.Application.Queries.QuotationRevisions;
using UglyToad.PdfPig;
using Xunit;

namespace TravelService.Tests;

public sealed class PdfDocumentRendererTests
{
    private readonly QuestPdfDocumentRenderer _renderer = new();

    [Fact]
    public void RenderQuotationRevisionPdf_GeneratesValidPdf_WithMeaningfulQuoteContent()
    {
        // QuotationRevisionReadModel is a settable-property class now (Dapper-friendly),
        // not a positional record — build with object initializer.
        var revision = new QuotationRevisionReadModel
        {
            Id = Guid.NewGuid(),
            QuotationId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            RevisionNumber = 7,
            Status = "Draft",
            CustomerContactId = Guid.NewGuid(),
            CustomerName = "Ava Patel",
            Title = "Summer escape in Bali",
            Destination = "Bali",
            TravelDate = new DateTimeOffset(2026, 7, 10, 0, 0, 0, TimeSpan.Zero),
            ReturnDate = new DateTimeOffset(2026, 7, 18, 0, 0, 0, TimeSpan.Zero),
            Travellers = 2,
            Currency = "USD",
            Notes = "Internal only",
            VisibleNotes = "Includes breakfast and airport transfers.",
            InternalNotes = "Do not show",
            ValidUntil = new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero),
            SubtotalAmount = 2400m,
            TaxAmount = 120m,
            TotalAmount = 2520m,
            CreatedByUserId = null,
            CreatedAt = new DateTimeOffset(2026, 4, 18, 0, 0, 0, TimeSpan.Zero),
            LineItems = new[]
            {
                new QuotationRevisionLineItemReadModel(Guid.NewGuid(), "Beach resort stay", 1, 1800m, "USD", 1, 1800m),
                new QuotationRevisionLineItemReadModel(Guid.NewGuid(), "Private transfer", 2, 300m, "USD", 2, 600m),
            },
            Attachments = Array.Empty<QuotationRevisionAttachmentReadModel>(),
        };

        var bytes = _renderer.RenderQuotationRevisionPdf(revision);

        AssertPdf(bytes, text =>
        {
            text = Normalize(text);
            text.Should().Contain(Normalize("Summer escape in Bali"));
            text.Should().Contain(Normalize("Ava Patel"));
            text.Should().Contain(Normalize("Bali"));
            text.Should().Contain(Normalize("Beach resort stay"));
            text.Should().Contain(Normalize("Private transfer"));
            text.Should().Contain(Normalize("USD 2520.00"));
            text.Should().Contain(Normalize("Includes breakfast and airport transfers."));
        });
    }

    [Fact]
    public void RenderItineraryPdf_GeneratesValidPdf_WithMeaningfulItineraryContent()
    {
        var itinerary = new ItineraryReadModel(
            Guid.Parse("11111111-2222-3333-4444-555555555555"),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Noah Fernandes",
            "Paris anniversary itinerary",
            "Paris",
            new DateTimeOffset(2026, 9, 2, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 9, 9, 0, 0, 0, TimeSpan.Zero),
            2,
            "EUR",
            Guid.NewGuid(),
            Guid.NewGuid(),
            true,
            "Booking",
            "Confirmed",
            3899.50m,
            new DateTimeOffset(2026, 4, 17, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 4, 18, 0, 0, 0, TimeSpan.Zero));

        var bytes = _renderer.RenderItineraryPdf(itinerary);

        AssertPdf(bytes, text =>
        {
            text = Normalize(text);
            text.Should().Contain(Normalize("Paris anniversary"));
            text.Should().Contain(Normalize("Noah Fernandes"));
            text.Should().Contain(Normalize("Paris"));
            text.Should().Contain(Normalize("Trip summary"));
            text.Should().Contain(Normalize("Travel window"));
            text.Should().Contain(Normalize("EUR 3899.50"));
            text.Should().Contain("status");
        });
    }

    private static void AssertPdf(byte[] bytes, Action<string> assertText)
    {
        Encoding.ASCII.GetString(bytes[..Math.Min(bytes.Length, 8)]).Should().StartWith("%PDF-");

        using var stream = new MemoryStream(bytes);
        using var document = PdfDocument.Open(stream);
        var text = string.Join("\n", document.GetPages().Select(page => page.Text));
        assertText(text);
    }

    private static string Normalize(string text)
    {
        var chars = text
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) || ch == '.' ? ch : ' ')
            .ToArray();

        return string.Join(' ', new string(chars).Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
