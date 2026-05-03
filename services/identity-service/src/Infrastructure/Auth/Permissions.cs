namespace IdentityService.Infrastructure.Auth;

public static class Permissions
{
    public static class Identity
    {
        public const string UsersManage = "identity.users.manage";
        public const string RolesManage = "identity.roles.manage";
        public const string AuditRead = "identity.audit.read";
        public const string SettingsManage = "identity.settings.manage";
        public const string TenantManage = "identity.tenant.manage";
    }

    public static class Branding
    {
        public const string ThemeManage = "branding.theme.manage";
    }

    public static class Travel
    {
        public const string WorkflowHubRead = "travel.workflowhub.read";
        public const string QuotationRead = "travel.quotation.read";
        public const string QuotationWrite = "travel.quotation.write";
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
        Identity.SettingsManage,
        Identity.TenantManage,
        Branding.ThemeManage,
        Travel.WorkflowHubRead,
        Travel.QuotationRead,
        Travel.QuotationWrite,
        Billing.InvoicesRead,
        Communication.LogsRead,
        Communication.NotificationSend,
        Communication.TemplatesManage
    ];
}
