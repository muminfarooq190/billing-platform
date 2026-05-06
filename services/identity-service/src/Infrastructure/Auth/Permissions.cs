namespace IdentityService.Infrastructure.Auth;

public static class Permissions
{
    public static class Identity
    {
        public const string UsersManage = "identity.users.manage";
        public const string RolesManage = "identity.roles.manage";
        public const string AuditRead = "identity.audit.read";
        public const string SettingsRead = "identity.settings.read";
        public const string SettingsManage = "identity.settings.manage";
        public const string TenantManage = "identity.tenant.manage";
    }

    public static class Branding
    {
        public const string ThemeRead = "branding.theme.read";
        public const string ThemeManage = "branding.theme.manage";
    }

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

    public static class Billing
    {
        public const string InvoicesRead = "billing.invoices.read";
    }

    public static class Communication
    {
        public const string LogsRead = "communication.logs.read";
        public const string NotificationSend = "communication.notification.send";
        public const string TemplatesManage = "communication.templates.manage";
    }

    public static readonly string[] All =
    [
        Identity.UsersManage,
        Identity.RolesManage,
        Identity.AuditRead,
        Identity.SettingsRead,
        Identity.SettingsManage,
        Identity.TenantManage,
        Branding.ThemeRead,
        Branding.ThemeManage,
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
        Billing.InvoicesRead,
        Communication.LogsRead,
        Communication.NotificationSend,
        Communication.TemplatesManage
    ];
}
