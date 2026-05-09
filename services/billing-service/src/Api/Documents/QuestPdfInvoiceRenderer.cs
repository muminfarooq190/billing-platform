using System.Globalization;
using BillingService.Application.ReadModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BillingService.Api.Documents;

public sealed class QuestPdfInvoiceRenderer : IInvoicePdfRenderer
{
    static QuestPdfInvoiceRenderer()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] RenderInvoicePdf(InvoiceReadModel invoice)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(t => t.FontSize(11).FontColor(Colors.Grey.Darken3));

                page.Header().Element(c => ComposeHeader(c, invoice));
                page.Content().Element(c => ComposeBody(c, invoice));
                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Page ").FontSize(9);
                    t.CurrentPageNumber().FontSize(9);
                    t.Span(" of ").FontSize(9);
                    t.TotalPages().FontSize(9);
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, InvoiceReadModel invoice)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Voyara").FontSize(20).Bold().FontColor(Colors.Blue.Medium);
                col.Item().Text("Tenant operating SaaS").FontSize(10).FontColor(Colors.Grey.Darken1);
            });

            row.RelativeItem().AlignRight().Column(col =>
            {
                col.Item().Text("INVOICE").FontSize(18).Bold().FontColor(Colors.Grey.Darken2);
                col.Item().Text(invoice.InvoiceNumber).FontSize(11).FontColor(Colors.Grey.Darken1);
                col.Item().PaddingTop(4).Text($"Status: {invoice.Status}").FontSize(10).FontColor(StatusColor(invoice.Status));
            });
        });
    }

    private static void ComposeBody(IContainer container, InvoiceReadModel invoice)
    {
        container.PaddingVertical(16).Column(col =>
        {
            col.Spacing(16);

            col.Item().Element(c => ComposeKeyFacts(c, invoice));
            col.Item().Element(c => ComposeAmountSection(c, invoice));
        });
    }

    private static void ComposeKeyFacts(IContainer container, InvoiceReadModel invoice)
    {
        var currency = string.IsNullOrWhiteSpace(invoice.Currency) ? "USD" : invoice.Currency!;

        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(12).Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                Fact(c, "Invoice number", invoice.InvoiceNumber);
                Fact(c, "Status", invoice.Status);
                Fact(c, "Currency", currency);
            });
            row.RelativeItem().Column(c =>
            {
                Fact(c, "Issue date", invoice.DueDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture));
                Fact(c, "Due date", invoice.DueDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture));
                Fact(c, "Paid at", invoice.PaidAt?.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) ?? "Not yet");
            });
        });
    }

    private static void Fact(ColumnDescriptor col, string label, string value)
    {
        col.Item().PaddingVertical(2).Row(row =>
        {
            row.ConstantItem(110).Text(label).FontSize(9).FontColor(Colors.Grey.Darken1);
            row.RelativeItem().Text(value).FontSize(11).Bold();
        });
    }

    private static void ComposeAmountSection(IContainer container, InvoiceReadModel invoice)
    {
        var currency = string.IsNullOrWhiteSpace(invoice.Currency) ? "USD" : invoice.Currency!;
        container.Background(Colors.Grey.Lighten4).Padding(16).Column(col =>
        {
            col.Spacing(6);
            AmountRow(col, "Total", invoice.TotalAmount, currency, bold: true);
            AmountRow(col, "Paid", invoice.PaidAmount, currency);
            AmountRow(col, "Outstanding", invoice.DueAmount, currency, bold: true, color: Colors.Red.Medium);
        });
    }

    private static void AmountRow(ColumnDescriptor col, string label, decimal? amount, string currency, bool bold = false, string? color = null)
    {
        col.Item().Row(row =>
        {
            row.RelativeItem().Text(label).FontSize(11).FontColor(Colors.Grey.Darken2);
            var text = row.ConstantItem(160).AlignRight().Text(FormatMoney(amount, currency));
            text.FontSize(13);
            if (bold) text.Bold();
            if (color is not null) text.FontColor(color);
        });
    }

    private static string FormatMoney(decimal? amount, string currency)
    {
        if (amount is null) return "—";
        return $"{currency} {amount.Value.ToString("N2", CultureInfo.InvariantCulture)}";
    }

    private static string StatusColor(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "paid" => Colors.Green.Medium,
            "overdue" => Colors.Red.Medium,
            "voided" => Colors.Grey.Darken1,
            _ => Colors.Blue.Medium,
        };
    }
}
