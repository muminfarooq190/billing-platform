using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TravelService.Application.Queries.GetItineraryById;
using TravelService.Application.Queries.QuotationRevisions;

namespace TravelService.Api.Documents;

public sealed class QuestPdfDocumentRenderer : IPdfDocumentRenderer
{
    private static readonly string[] QuotationTableHeaders = ["Description", "Qty", "Unit Price", "Line Total"];

    static QuestPdfDocumentRenderer()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] RenderQuotationRevisionPdf(QuotationRevisionReadModel revision)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurePage(page, $"Quotation #{revision.RevisionNumber}");
                page.Content().Column(column =>
                {
                    column.Spacing(16);
                    column.Item().Element(c => ComposeHeader(c, "Travel quotation", revision.Title, revision.CustomerName));
                    column.Item().Element(c => ComposeKeyFacts(c, [
                        ("Destination", revision.Destination),
                        ("Travel dates", $"{FormatDate(revision.TravelDate)} - {FormatDate(revision.ReturnDate)}"),
                        ("Travellers", revision.Travellers.ToString()),
                        ("Currency", revision.Currency),
                        ("Valid until", FormatDate(revision.ValidUntil)),
                        ("Status", revision.Status)
                    ]));
                    column.Item().Element(c => ComposeQuotationTable(c, revision));
                    column.Item().Element(c => ComposeTotals(c, revision.Currency, revision.SubtotalAmount, revision.TaxAmount, revision.TotalAmount));

                    if (!string.IsNullOrWhiteSpace(revision.VisibleNotes))
                        column.Item().Element(c => ComposeNotes(c, "Notes", revision.VisibleNotes));
                });
            });
        }).GeneratePdf();
    }

    public byte[] RenderItineraryPdf(ItineraryReadModel itinerary)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurePage(page, $"Itinerary {itinerary.Id:D}");
                page.Content().Column(column =>
                {
                    column.Spacing(16);
                    column.Item().Element(c => ComposeHeader(c, "Travel itinerary", itinerary.Title, itinerary.CustomerName));
                    column.Item().Element(c => ComposeKeyFacts(c, [
                        ("Destination", itinerary.Destination),
                        ("Travel dates", $"{FormatDate(itinerary.StartDate)} - {FormatDate(itinerary.EndDate)}"),
                        ("Travellers", itinerary.Travellers.ToString()),
                        ("Status", itinerary.Status),
                        ("Currency", itinerary.Currency),
                        ("Estimated cost", FormatMoney(itinerary.Currency, itinerary.TotalCost))
                    ]));
                    column.Item().Element(c => ComposeParagraphCard(c, "Trip summary",
                        $"{itinerary.CustomerName} is scheduled for {itinerary.Destination} from {FormatDate(itinerary.StartDate)} to {FormatDate(itinerary.EndDate)} for {itinerary.Travellers} traveller(s). Current itinerary status is {itinerary.Status.ToLowerInvariant()} and the estimated trip cost is {FormatMoney(itinerary.Currency, itinerary.TotalCost)}."));
                    column.Item().Element(c => ComposeParagraphCard(c, "What this document covers",
                        "This customer-facing itinerary summarizes the confirmed travel window, destination, traveller count, and current booking status. Share it alongside vouchers, hotel confirmations, and transport details as those become available."));
                    column.Item().Element(c => ComposeParagraphCard(c, "Reference details",
                        $"Booking ownership: {itinerary.OwnershipType}. Generated from itinerary record {itinerary.Id:D}. Last updated {itinerary.UpdatedAt:dd MMM yyyy}."));
                });
            });
        }).GeneratePdf();
    }

    private static void ConfigurePage(PageDescriptor page, string documentTitle)
    {
        page.Margin(32);
        page.Size(PageSizes.A4);
        page.DefaultTextStyle(x => x.FontSize(11));
        page.Header().Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("Voyara").SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);
                column.Item().Text(documentTitle).FontColor(Colors.Grey.Darken1);
            });
            row.ConstantItem(110).AlignRight().Text($"Generated {DateTime.UtcNow:dd MMM yyyy}").FontColor(Colors.Grey.Darken1);
        });
        page.Footer().AlignCenter().DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken1)).Text(text =>
        {
            text.Span("Customer document • Page ");
            text.CurrentPageNumber();
            text.Span(" of ");
            text.TotalPages();
        });
    }

    private static void ComposeHeader(IContainer container, string eyebrow, string title, string customerName)
    {
        container.Background(Colors.Grey.Lighten4).Padding(16).Column(column =>
        {
            column.Spacing(4);
            column.Item().Text(eyebrow.ToUpperInvariant()).SemiBold().FontSize(10).FontColor(Colors.Blue.Darken2);
            column.Item().Text(title).SemiBold().FontSize(18);
            column.Item().Text($"Prepared for {customerName}").FontColor(Colors.Grey.Darken2);
        });
    }

    private static void ComposeKeyFacts(IContainer container, IReadOnlyList<(string Label, string Value)> items)
    {
        container.Column(column =>
        {
            column.Item().Text("Overview").SemiBold().FontSize(13);
            column.Item().PaddingTop(8).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                foreach (var (label, value) in items)
                {
                    table.Cell().Element(FactCard).Column(card =>
                    {
                        card.Spacing(2);
                        card.Item().Text(label).SemiBold().FontSize(10).FontColor(Colors.Grey.Darken1);
                        card.Item().Text(string.IsNullOrWhiteSpace(value) ? "-" : value);
                    });
                }
            });
        });
    }

    private static void ComposeQuotationTable(IContainer container, QuotationRevisionReadModel revision)
    {
        container.Column(column =>
        {
            column.Item().Text("Pricing details").SemiBold().FontSize(13);
            column.Item().PaddingTop(8).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);
                    columns.RelativeColumn();
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    foreach (var heading in QuotationTableHeaders)
                        header.Cell().Element(TableHeaderCell).Text(heading).SemiBold();
                });

                if (revision.LineItems.Count == 0)
                {
                    table.Cell().ColumnSpan(4).Element(TableBodyCell).Text("No line items recorded for this quotation revision.");
                    return;
                }

                foreach (var item in revision.LineItems.OrderBy(x => x.SortOrder))
                {
                    table.Cell().Element(TableBodyCell).Text(item.Description);
                    table.Cell().Element(TableBodyCell).AlignRight().Text(item.Quantity.ToString());
                    table.Cell().Element(TableBodyCell).AlignRight().Text(FormatMoney(item.Currency, item.UnitPriceAmount));
                    table.Cell().Element(TableBodyCell).AlignRight().Text(FormatMoney(item.Currency, item.LineTotal));
                }
            });
        });
    }

    private static void ComposeTotals(IContainer container, string currency, decimal subtotal, decimal tax, decimal total)
    {
        container.AlignRight().Width(220).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            AddTotalRow(table, "Subtotal", FormatMoney(currency, subtotal));
            AddTotalRow(table, "Tax", FormatMoney(currency, tax));
            AddTotalRow(table, "Total", FormatMoney(currency, total), true);
        });
    }

    private static void AddTotalRow(TableDescriptor table, string label, string value, bool emphasize = false)
    {
        var labelCell = table.Cell().Element(c => SummaryCell(c, emphasize));
        if (emphasize)
            labelCell.Text(label).SemiBold();
        else
            labelCell.Text(label);

        var valueCell = table.Cell().Element(c => SummaryCell(c, emphasize)).AlignRight();
        if (emphasize)
            valueCell.Text(value).SemiBold();
        else
            valueCell.Text(value);
    }

    private static IContainer SummaryCell(IContainer container, bool emphasize)
    {
        var color = emphasize ? Colors.Blue.Lighten4 : Colors.White;
        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Background(color).Padding(8);
    }

    private static IContainer FactCard(IContainer container)
        => container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10);

    private static void ComposeNotes(IContainer container, string title, string body)
    {
        ComposeParagraphCard(container, title, body);
    }

    private static void ComposeParagraphCard(IContainer container, string title, string body)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(12).Column(column =>
        {
            column.Spacing(6);
            column.Item().Text(title).SemiBold().FontSize(13);
            column.Item().Text(string.IsNullOrWhiteSpace(body) ? "-" : body);
        });
    }

    private static IContainer TableHeaderCell(IContainer container)
        => container.Background(Colors.Blue.Lighten4).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8);

    private static IContainer TableBodyCell(IContainer container)
        => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(8);

    private static string FormatDate(DateTimeOffset value) => value.ToString("dd MMM yyyy");

    private static string FormatMoney(string currency, decimal amount) => $"{currency} {amount:0.00}";
}
