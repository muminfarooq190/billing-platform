namespace TravelService.Api.Documents;

/// <summary>
/// Brand/template inputs the PDF renderer applies on top of a quotation
/// or itinerary. Composed from the active TravelTemplate for the document's
/// context (Concept/Quote/Itinerary) plus any per-revision policy fields.
/// All fields are optional — renderer falls back to neutral defaults when null.
/// </summary>
public sealed record PdfBranding(
    string DisplayName,
    string? Tagline,
    string? BannerUrl,
    string AccentColor,
    string? PrimaryColor,
    string? FontFamily,
    IReadOnlyList<PdfBrandingSection> Sections,
    IReadOnlyList<string> Inclusions,
    IReadOnlyList<string> Exclusions,
    string? PaymentTerms,
    string? CancellationPolicy);

public sealed record PdfBrandingSection(string Id, string Label, string? Hint);
