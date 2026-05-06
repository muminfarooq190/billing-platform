namespace TravelService.Api.Auth;

public static class Permissions
{
    public static class Travel
    {
        public const string WorkflowHubRead = "travel.workflowhub.read";
        public const string InquiriesRead = "travel.inquiries.read";
        public const string InquiriesWrite = "travel.inquiries.write";
        public const string ContactsRead = "travel.contacts.read";
        public const string ContactsWrite = "travel.contacts.write";
        public const string FollowUpsRead = "travel.followups.read";
        public const string FollowUpsWrite = "travel.followups.write";
        public const string BookingsRead = "travel.bookings.read";
        public const string BookingsWrite = "travel.bookings.write";
        public const string ItinerariesRead = "travel.itineraries.read";
        public const string ItinerariesWrite = "travel.itineraries.write";
        public const string TimelineRead = "travel.timeline.read";
        public const string NotesRead = "travel.notes.read";
        public const string NotesWrite = "travel.notes.write";
        public const string DocumentsRead = "travel.documents.read";
        public const string QuotationRead = "travel.quotation.read";
        public const string QuotationsRead = "travel.quotations.read";
        public const string QuotationWrite = "travel.quotation.write";
        public const string AuditRead = "travel.audit.read";
        public const string TemplatesRead = "travel.templates.read";
        public const string TemplatesWrite = "travel.templates.write";
    }

    public static readonly string[] All =
    [
        Travel.WorkflowHubRead,
        Travel.InquiriesRead,
        Travel.InquiriesWrite,
        Travel.ContactsRead,
        Travel.ContactsWrite,
        Travel.FollowUpsRead,
        Travel.FollowUpsWrite,
        Travel.BookingsRead,
        Travel.BookingsWrite,
        Travel.ItinerariesRead,
        Travel.ItinerariesWrite,
        Travel.TimelineRead,
        Travel.NotesRead,
        Travel.NotesWrite,
        Travel.DocumentsRead,
        Travel.QuotationRead,
        Travel.QuotationsRead,
        Travel.QuotationWrite,
        Travel.AuditRead,
        Travel.TemplatesRead,
        Travel.TemplatesWrite,
    ];
}
