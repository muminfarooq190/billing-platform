using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TravelService.Application.Queries.GetItineraryById;
using TravelService.Application.Queries.QuotationRevisions;

namespace TravelService.Api.Documents;

/// <summary>
/// "Sophisticated Voyager" PDF renderer. Two-stage layout for quotations:
///   1. Full-bleed cover page (banner image, brand mark, customer + dates ribbon).
///   2. Editorial body with refined typography, numbered template sections,
///      hairline-tabled line items, inclusions/exclusions comparison block,
///      and policy note cards.
///
/// Falls back to a single-page composition for itineraries.
/// Branding (accent color, banner URL, sections, policy text) is supplied by
/// <see cref="PdfBranding"/>; null branding uses Voyara defaults.
/// </summary>
public sealed class QuestPdfDocumentRenderer : IPdfDocumentRenderer
{
    private const string DefaultAccent     = "#436653";  // Voyara forest green
    private const string DefaultPrimary    = "#041627";  // Voyara deep navy
    private const string DefaultMuted      = "#74777D";  // M3 outline
    private const string DefaultBgSubtle   = "#F3F4F5";  // surface-container-low
    private const string DefaultBgInverse  = "#FFFFFFCC";// overlay on hero
    private const string DefaultFont       = "Inter";

    private readonly IHttpClientFactory? httpClientFactory;

    public QuestPdfDocumentRenderer(IHttpClientFactory? httpClientFactory = null)
    {
        this.httpClientFactory = httpClientFactory;
    }

    static QuestPdfDocumentRenderer()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] RenderQuotationRevisionPdf(QuotationRevisionReadModel revision, PdfBranding? branding = null)
    {
        var ctx = ResolveContext(branding);

        return Document.Create(container =>
        {
            // ── Page 1: cover ──────────────────────────────────────
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontFamily(ctx.FontFamily).FontColor(ctx.PrimaryColor));
                page.Content().Element(c => ComposeCover(c, ctx, revision));
            });

            // ── Page 2+: body ──────────────────────────────────────
            container.Page(page =>
            {
                ConfigureBodyPage(page, ctx);
                page.Content().Column(column =>
                {
                    column.Spacing(28);
                    column.Item().Element(c => ComposeRibbon(c, ctx,
                        ("Destination", revision.Destination),
                        ("Travel window", $"{FormatDate(revision.TravelDate)} → {FormatDate(revision.ReturnDate)}"),
                        ("Travellers", revision.Travellers.ToString()),
                        ("Total", FormatMoney(revision.Currency, revision.TotalAmount))));

                    if (ctx.Sections.Count > 0)
                        column.Item().Element(c => ComposeTemplateSections(c, ctx));

                    column.Item().Element(c => ComposeLineItemsBlock(c, ctx, revision));
                    column.Item().Element(c => ComposeTotalsBlock(c, ctx, revision));

                    if (ctx.Inclusions.Count > 0 || ctx.Exclusions.Count > 0)
                        column.Item().Element(c => ComposeInclusionsExclusions(c, ctx));

                    if (!string.IsNullOrWhiteSpace(ctx.PaymentTerms))
                        column.Item().Element(c => ComposeNoteCard(c, ctx, "Payment terms", ctx.PaymentTerms!));

                    if (!string.IsNullOrWhiteSpace(ctx.CancellationPolicy))
                        column.Item().Element(c => ComposeNoteCard(c, ctx, "Cancellation policy", ctx.CancellationPolicy!));

                    if (!string.IsNullOrWhiteSpace(revision.VisibleNotes))
                        column.Item().Element(c => ComposeNoteCard(c, ctx, "Notes", revision.VisibleNotes));
                });
            });
        }).GeneratePdf();
    }

    public byte[] RenderItineraryPdf(ItineraryReadModel itinerary, PdfBranding? branding = null)
    {
        var ctx = ResolveContext(branding);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontFamily(ctx.FontFamily).FontColor(ctx.PrimaryColor));
                page.Content().Element(c => ComposeItineraryCover(c, ctx, itinerary));
            });

            container.Page(page =>
            {
                ConfigureBodyPage(page, ctx);
                page.Content().Column(column =>
                {
                    column.Spacing(28);
                    column.Item().Element(c => ComposeRibbon(c, ctx,
                        ("Destination", itinerary.Destination),
                        ("Travel window", $"{FormatDate(itinerary.StartDate)} → {FormatDate(itinerary.EndDate)}"),
                        ("Travellers", itinerary.Travellers.ToString()),
                        ("Status", itinerary.Status)));

                    if (ctx.Sections.Count > 0)
                        column.Item().Element(c => ComposeTemplateSections(c, ctx));

                    column.Item().Element(c => ComposeNoteCard(c, ctx, "Trip summary",
                        $"{itinerary.CustomerName} travels to {itinerary.Destination} from {FormatDate(itinerary.StartDate)} to {FormatDate(itinerary.EndDate)} with {itinerary.Travellers} traveller(s). Estimated investment {FormatMoney(itinerary.Currency, itinerary.TotalCost)}."));

                    if (ctx.Inclusions.Count > 0 || ctx.Exclusions.Count > 0)
                        column.Item().Element(c => ComposeInclusionsExclusions(c, ctx));
                });
            });
        }).GeneratePdf();
    }

    // ═══════════════════════════════════════════════════════════
    // Cover
    // ═══════════════════════════════════════════════════════════

    private static void ComposeCover(IContainer container, RenderContext ctx, QuotationRevisionReadModel revision)
    {
        container.Background(ctx.PrimaryColor).Column(col =>
        {
            // Hero band (banner image OR solid accent gradient)
            col.Item().Height(360).Background(ctx.AccentColor).Layers(layers =>
            {
                if (ctx.BannerBytes is not null)
                {
                    layers.PrimaryLayer().Image(ctx.BannerBytes).FitArea();
                    layers.Layer().Background("#04162780"); // navy tint overlay for legibility
                }
                else
                {
                    layers.PrimaryLayer().Background(ctx.AccentColor);
                }

                layers.Layer().Padding(48).Column(stack =>
                {
                    stack.Spacing(12);
                    stack.Item().Text(ctx.DisplayName.ToUpperInvariant()).SemiBold().FontSize(11).FontColor("#FFFFFF");
                    if (!string.IsNullOrWhiteSpace(ctx.Tagline))
                        stack.Item().Text(ctx.Tagline!).Italic().FontSize(11).FontColor("#FFFFFFAA");
                });

                layers.Layer().AlignBottom().Padding(48).Column(stack =>
                {
                    stack.Spacing(8);
                    stack.Item().Text("Travel Quotation").FontSize(13).SemiBold().FontColor("#FFFFFFB3");
                    stack.Item().Text(revision.Title).FontSize(34).SemiBold().FontColor("#FFFFFF").Style(TextStyle.Default);
                    stack.Item().Text($"Prepared for {revision.CustomerName}").FontSize(13).FontColor("#FFFFFFD9");
                });
            });

            // Cover ribbon — 4 column stat block on solid navy
            col.Item().Background(ctx.PrimaryColor).PaddingVertical(28).PaddingHorizontal(48).Row(row =>
            {
                CoverStat(row.RelativeItem(), "Destination", revision.Destination, ctx);
                CoverStat(row.RelativeItem(), "Travel dates", $"{FormatDate(revision.TravelDate)} - {FormatDate(revision.ReturnDate)}", ctx);
                CoverStat(row.RelativeItem(), "Travellers", revision.Travellers.ToString(), ctx);
                CoverStat(row.RelativeItem(), "Total", FormatMoney(revision.Currency, revision.TotalAmount), ctx, emphasize: true);
            });

            // Metadata footer on cover
            col.Item().Background(ctx.PrimaryColor).PaddingVertical(20).PaddingHorizontal(48).Row(row =>
            {
                row.RelativeItem().Text($"Revision #{revision.RevisionNumber} · {FormatDate(revision.CreatedAt)}").FontSize(10).FontColor("#FFFFFF99");
                row.RelativeItem().AlignRight().Text($"Valid until {FormatDate(revision.ValidUntil)}").FontSize(10).FontColor("#FFFFFF99");
            });
        });
    }

    private static void ComposeItineraryCover(IContainer container, RenderContext ctx, ItineraryReadModel itinerary)
    {
        container.Background(ctx.PrimaryColor).Column(col =>
        {
            col.Item().Height(360).Background(ctx.AccentColor).Layers(layers =>
            {
                if (ctx.BannerBytes is not null)
                {
                    layers.PrimaryLayer().Image(ctx.BannerBytes).FitArea();
                    layers.Layer().Background("#04162780");
                }
                else
                {
                    layers.PrimaryLayer().Background(ctx.AccentColor);
                }

                layers.Layer().Padding(48).Column(stack =>
                {
                    stack.Spacing(12);
                    stack.Item().Text(ctx.DisplayName.ToUpperInvariant()).SemiBold().FontSize(11).FontColor("#FFFFFF");
                    if (!string.IsNullOrWhiteSpace(ctx.Tagline))
                        stack.Item().Text(ctx.Tagline!).Italic().FontSize(11).FontColor("#FFFFFFAA");
                });

                layers.Layer().AlignBottom().Padding(48).Column(stack =>
                {
                    stack.Spacing(8);
                    stack.Item().Text("Travel Itinerary").FontSize(13).SemiBold().FontColor("#FFFFFFB3");
                    stack.Item().Text(itinerary.Title).FontSize(34).SemiBold().FontColor("#FFFFFF");
                    stack.Item().Text($"Prepared for {itinerary.CustomerName}").FontSize(13).FontColor("#FFFFFFD9");
                });
            });

            col.Item().Background(ctx.PrimaryColor).PaddingVertical(28).PaddingHorizontal(48).Row(row =>
            {
                CoverStat(row.RelativeItem(), "Destination", itinerary.Destination, ctx);
                CoverStat(row.RelativeItem(), "Travel dates", $"{FormatDate(itinerary.StartDate)} - {FormatDate(itinerary.EndDate)}", ctx);
                CoverStat(row.RelativeItem(), "Travellers", itinerary.Travellers.ToString(), ctx);
                CoverStat(row.RelativeItem(), "Status", itinerary.Status, ctx, emphasize: true);
            });
        });
    }

    private static void CoverStat(IContainer container, string label, string value, RenderContext ctx, bool emphasize = false)
    {
        container.Column(col =>
        {
            col.Spacing(4);
            col.Item().Text(label.ToUpperInvariant()).FontSize(9).FontColor("#FFFFFF99").LetterSpacing(0.06f);
            col.Item().Text(string.IsNullOrWhiteSpace(value) ? "—" : value).FontSize(emphasize ? 16 : 13).SemiBold().FontColor("#FFFFFF");
        });
    }

    // ═══════════════════════════════════════════════════════════
    // Body shell
    // ═══════════════════════════════════════════════════════════

    private static void ConfigureBodyPage(PageDescriptor page, RenderContext ctx)
    {
        page.Size(PageSizes.A4);
        page.MarginHorizontal(56);
        page.MarginVertical(48);
        page.DefaultTextStyle(x => x.FontFamily(ctx.FontFamily).FontSize(10.5f).LineHeight(1.45f).FontColor(ctx.PrimaryColor));

        page.Header().Row(row =>
        {
            row.RelativeItem().Text(ctx.DisplayName).SemiBold().FontSize(11).FontColor(ctx.AccentColor);
            row.RelativeItem().AlignRight().Text($"{DateTime.UtcNow:dd MMM yyyy}").FontSize(9).FontColor(DefaultMuted);
        });

        page.Footer().PaddingTop(8).Row(row =>
        {
            row.RelativeItem().Text(ctx.DisplayName).FontSize(8.5f).FontColor(DefaultMuted);
            row.RelativeItem().AlignRight().Text(text =>
            {
                text.DefaultTextStyle(x => x.FontSize(8.5f).FontColor(DefaultMuted));
                text.Span("Page ");
                text.CurrentPageNumber();
                text.Span(" of ");
                text.TotalPages();
            });
        });
    }

    // ═══════════════════════════════════════════════════════════
    // Body content blocks
    // ═══════════════════════════════════════════════════════════

    private static void ComposeRibbon(IContainer container, RenderContext ctx, params (string Label, string Value)[] cells)
    {
        container.Background(DefaultBgSubtle).PaddingVertical(16).PaddingHorizontal(20).Row(row =>
        {
            for (var i = 0; i < cells.Length; i++)
            {
                var (label, value) = cells[i];
                var cell = row.RelativeItem();
                if (i > 0)
                {
                    cell.BorderLeft(1).BorderColor("#E1E3E4").PaddingLeft(16).Column(col =>
                    {
                        col.Spacing(2);
                        col.Item().Text(label.ToUpperInvariant()).FontSize(8.5f).FontColor(DefaultMuted).LetterSpacing(0.06f);
                        col.Item().Text(string.IsNullOrWhiteSpace(value) ? "—" : value).FontSize(11).SemiBold().FontColor(ctx.PrimaryColor);
                    });
                }
                else
                {
                    cell.Column(col =>
                    {
                        col.Spacing(2);
                        col.Item().Text(label.ToUpperInvariant()).FontSize(8.5f).FontColor(DefaultMuted).LetterSpacing(0.06f);
                        col.Item().Text(string.IsNullOrWhiteSpace(value) ? "—" : value).FontSize(11).SemiBold().FontColor(ctx.PrimaryColor);
                    });
                }
            }
        });
    }

    private static void ComposeTemplateSections(IContainer container, RenderContext ctx)
    {
        container.Column(col =>
        {
            col.Item().Element(c => SectionEyebrow(c, ctx, "Itinerary outline"));
            col.Item().PaddingTop(8).Column(list =>
            {
                list.Spacing(14);
                var idx = 1;
                foreach (var section in ctx.Sections)
                {
                    list.Item().Row(row =>
                    {
                        row.ConstantItem(36).Text(idx.ToString("00")).SemiBold().FontSize(14).FontColor(ctx.AccentColor);
                        row.RelativeItem().PaddingLeft(10).BorderLeft(2).BorderColor(ctx.AccentColor).PaddingLeft(14).Column(stack =>
                        {
                            stack.Item().Text(section.Label).SemiBold().FontSize(12).FontColor(ctx.PrimaryColor);
                            if (!string.IsNullOrWhiteSpace(section.Hint))
                                stack.Item().Text(section.Hint!).FontSize(10).FontColor(DefaultMuted);
                        });
                    });
                    idx++;
                }
            });
        });
    }

    private static void ComposeLineItemsBlock(IContainer container, RenderContext ctx, QuotationRevisionReadModel revision)
    {
        container.Column(col =>
        {
            col.Item().Element(c => SectionEyebrow(c, ctx, "Pricing breakdown"));
            col.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(5);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    HeaderCell(header.Cell(), ctx, "Item");
                    HeaderCell(header.Cell(), ctx, "Qty", align: HorizontalAlignment.Right);
                    HeaderCell(header.Cell(), ctx, "Unit", align: HorizontalAlignment.Right);
                    HeaderCell(header.Cell(), ctx, "Line total", align: HorizontalAlignment.Right);
                });

                if (revision.LineItems.Count == 0)
                {
                    table.Cell().ColumnSpan(4).PaddingVertical(12).Text("No line items recorded for this revision.").FontColor(DefaultMuted).Italic();
                    return;
                }

                var rowIndex = 0;
                foreach (var item in revision.LineItems.OrderBy(x => x.SortOrder))
                {
                    var rowBg = rowIndex % 2 == 0 ? "#FFFFFF" : "#FAFBFB";
                    BodyCell(table.Cell(), rowBg).Text(item.Description);
                    BodyCell(table.Cell(), rowBg).AlignRight().Text(item.Quantity.ToString());
                    BodyCell(table.Cell(), rowBg).AlignRight().Text(FormatMoney(item.Currency, item.UnitPriceAmount));
                    BodyCell(table.Cell(), rowBg).AlignRight().Text(FormatMoney(item.Currency, item.LineTotal)).SemiBold();
                    rowIndex++;
                }
            });
        });
    }

    private static void ComposeTotalsBlock(IContainer container, RenderContext ctx, QuotationRevisionReadModel revision)
    {
        container.AlignRight().Width(260).Column(col =>
        {
            col.Spacing(4);
            TotalsLine(col.Item(), "Subtotal", FormatMoney(revision.Currency, revision.SubtotalAmount), ctx);
            TotalsLine(col.Item(), "Tax", FormatMoney(revision.Currency, revision.TaxAmount), ctx);
            col.Item().PaddingTop(8).BorderTop(1).BorderColor(ctx.AccentColor).PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Text("Grand total").SemiBold().FontSize(12).FontColor(ctx.PrimaryColor);
                row.RelativeItem().AlignRight().Text(FormatMoney(revision.Currency, revision.TotalAmount)).SemiBold().FontSize(16).FontColor(ctx.AccentColor);
            });
        });
    }

    private static void TotalsLine(IContainer container, string label, string value, RenderContext ctx)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text(label).FontSize(10.5f).FontColor(DefaultMuted);
            row.RelativeItem().AlignRight().Text(value).FontSize(11).FontColor(ctx.PrimaryColor);
        });
    }

    private static void ComposeInclusionsExclusions(IContainer container, RenderContext ctx)
    {
        container.Row(row =>
        {
            if (ctx.Inclusions.Count > 0)
            {
                row.RelativeItem().PaddingRight(ctx.Exclusions.Count > 0 ? 12 : 0).Column(col =>
                {
                    col.Item().Element(c => SectionEyebrow(c, ctx, "What's included"));
                    col.Item().PaddingTop(8).Column(list =>
                    {
                        list.Spacing(6);
                        foreach (var entry in ctx.Inclusions)
                            list.Item().Row(r =>
                            {
                                r.ConstantItem(16).Text("✓").FontColor(ctx.AccentColor).SemiBold();
                                r.RelativeItem().Text(entry).FontSize(10.5f);
                            });
                    });
                });
            }
            if (ctx.Exclusions.Count > 0)
            {
                row.RelativeItem().PaddingLeft(ctx.Inclusions.Count > 0 ? 12 : 0).Column(col =>
                {
                    col.Item().Element(c => SectionEyebrow(c, ctx, "Not included"));
                    col.Item().PaddingTop(8).Column(list =>
                    {
                        list.Spacing(6);
                        foreach (var entry in ctx.Exclusions)
                            list.Item().Row(r =>
                            {
                                r.ConstantItem(16).Text("✕").FontColor("#B91C1C").SemiBold();
                                r.RelativeItem().Text(entry).FontSize(10.5f).FontColor(DefaultMuted);
                            });
                    });
                });
            }
        });
    }

    private static void ComposeNoteCard(IContainer container, RenderContext ctx, string title, string body)
    {
        container.Row(row =>
        {
            row.ConstantItem(3).Background(ctx.AccentColor);
            row.RelativeItem().PaddingLeft(14).Column(col =>
            {
                col.Spacing(6);
                col.Item().Text(title.ToUpperInvariant()).FontSize(9).FontColor(DefaultMuted).LetterSpacing(0.06f);
                col.Item().Text(string.IsNullOrWhiteSpace(body) ? "—" : body).FontSize(10.5f).FontColor(ctx.PrimaryColor).LineHeight(1.55f);
            });
        });
    }

    // ═══════════════════════════════════════════════════════════
    // Small helpers
    // ═══════════════════════════════════════════════════════════

    private static void SectionEyebrow(IContainer container, RenderContext ctx, string label)
    {
        container.Row(row =>
        {
            row.ConstantItem(24).PaddingTop(6).BorderTop(1).BorderColor(ctx.AccentColor);
            row.AutoItem().PaddingLeft(8).Text(label.ToUpperInvariant()).SemiBold().FontSize(10).FontColor(ctx.AccentColor).LetterSpacing(0.1f);
        });
    }

    private static void HeaderCell(IContainer container, RenderContext ctx, string label, HorizontalAlignment align = HorizontalAlignment.Left)
    {
        var styled = container.PaddingVertical(8).PaddingHorizontal(8).BorderBottom(1).BorderColor("#E1E3E4");
        var aligned = align == HorizontalAlignment.Right ? styled.AlignRight() : styled;
        aligned.Text(label.ToUpperInvariant()).SemiBold().FontSize(9).FontColor(ctx.AccentColor).LetterSpacing(0.08f);
    }

    private static IContainer BodyCell(IContainer container, string bg)
        => container.Background(bg).PaddingVertical(10).PaddingHorizontal(8);

    private enum HorizontalAlignment { Left, Right }

    // ═══════════════════════════════════════════════════════════
    // Context resolution + utilities
    // ═══════════════════════════════════════════════════════════

    private RenderContext ResolveContext(PdfBranding? branding)
    {
        var accent = NormalizeHex(branding?.AccentColor, DefaultAccent);
        var primary = NormalizeHex(branding?.PrimaryColor, DefaultPrimary);
        var font = string.IsNullOrWhiteSpace(branding?.FontFamily) ? DefaultFont : branding!.FontFamily!;
        var displayName = string.IsNullOrWhiteSpace(branding?.DisplayName) ? "Voyara" : branding!.DisplayName;
        var tagline = branding?.Tagline;
        var bannerBytes = TryDownloadBanner(branding?.BannerUrl);
        var sections = branding?.Sections ?? Array.Empty<PdfBrandingSection>();
        var inclusions = branding?.Inclusions ?? Array.Empty<string>();
        var exclusions = branding?.Exclusions ?? Array.Empty<string>();

        return new RenderContext(accent, primary, font, displayName, tagline, bannerBytes, sections, inclusions, exclusions, branding?.PaymentTerms, branding?.CancellationPolicy);
    }

    private byte[]? TryDownloadBanner(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || httpClientFactory is null) return null;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return null;
        if (uri.Scheme is not ("http" or "https")) return null;
        try
        {
            using var client = httpClientFactory.CreateClient("pdf-assets");
            client.Timeout = TimeSpan.FromSeconds(5);
            using var response = client.GetAsync(uri).GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode) return null;
            return response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
        }
        catch
        {
            return null;
        }
    }

    private static string FormatDate(DateTimeOffset value) => value.ToString("dd MMM yyyy");

    private static string FormatMoney(string currency, decimal amount) => $"{currency} {amount:0.00}";

    private static string NormalizeHex(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value)) return fallback;
        var trimmed = value.Trim();
        if (!trimmed.StartsWith('#')) trimmed = "#" + trimmed;
        return (trimmed.Length is 4 or 7 or 9) && trimmed[1..].All(IsHexDigit) ? trimmed : fallback;
    }

    private static bool IsHexDigit(char c) => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');

    private sealed record RenderContext(
        string AccentColor,
        string PrimaryColor,
        string FontFamily,
        string DisplayName,
        string? Tagline,
        byte[]? BannerBytes,
        IReadOnlyList<PdfBrandingSection> Sections,
        IReadOnlyList<string> Inclusions,
        IReadOnlyList<string> Exclusions,
        string? PaymentTerms,
        string? CancellationPolicy);
}
