import { QuoteDecisionActions } from './QuoteDecisionActions';

type QuotePageProps = {
  params: {
    token: string;
  };
};

type PublicQuote = {
  quotationId: string;
  revisionId: string;
  customerName: string;
  title: string;
  destination: string;
  travelDate: string;
  returnDate: string;
  travellers: number;
  currency: string;
  visibleNotes: string;
  validUntil: string;
  totalAmount: number;
  sentAt?: string | null;
  lastViewedAt?: string | null;
  lineItems: Array<{
    description: string;
    quantity: number;
    unitPriceAmount: number;
    currency: string;
    sortOrder: number;
    lineTotal: number;
  }>;
  attachments: Array<{
    id: string;
    originalFileName: string;
    contentType: string;
    sizeBytes: number;
    attachmentType: string;
    caption?: string | null;
    sortOrder: number;
    readUrl: string;
  }>;
};

type TenantBranding = {
  displayName: string;
  primaryColor: string;
  secondaryColor: string;
  accentColor: string;
  textColor: string;
  backgroundColor: string;
  supportEmail?: string | null;
  supportPhone?: string | null;
  websiteUrl?: string | null;
  tagline?: string | null;
};

type TenantTemplateTheme = {
  id: string;
  templateScope: string;
  headerHtml?: string | null;
  footerHtml?: string | null;
  customCss?: string | null;
  logoAssetId?: string | null;
  backgroundAssetId?: string | null;
  settingsJson?: string | null;
};

const travelBaseUrl = process.env.TRAVEL_BASE_URL ?? 'http://localhost:8082';
const identityBaseUrl = process.env.IDENTITY_BASE_URL ?? 'http://localhost:8080';
const tenantId = process.env.DEFAULT_TENANT_ID;

async function getPublicQuote(token: string): Promise<PublicQuote | null> {
  const response = await fetch(`${travelBaseUrl}/travel/quotations/public/${token}`, { cache: 'no-store' });
  if (!response.ok) return null;
  return response.json();
}

async function getBranding(): Promise<TenantBranding | null> {
  const response = await fetch(`${identityBaseUrl}/tenant-branding`, {
    headers: tenantId ? { 'X-Tenant-Id': tenantId } : {},
    cache: 'no-store',
  });
  if (!response.ok) return null;
  return response.json();
}

async function getTemplateTheme(scope: string): Promise<TenantTemplateTheme | null> {
  const response = await fetch(`${identityBaseUrl}/tenant-branding/templates/${scope}`, {
    headers: tenantId ? { 'X-Tenant-Id': tenantId } : {},
    cache: 'no-store',
  });
  if (!response.ok) return null;
  return response.json();
}

export default async function PublicQuotePage({ params }: QuotePageProps) {
  const [quote, branding, theme] = await Promise.all([
    getPublicQuote(params.token),
    getBranding(),
    getTemplateTheme('QuotationPublicView'),
  ]);

  if (!quote) {
    return (
      <main style={{ padding: 32, maxWidth: 920, margin: '0 auto' }}>
        <h1>Quotation not found</h1>
        <p>This public quotation link is invalid or expired.</p>
      </main>
    );
  }

  return (
    <main
      style={{
        padding: 32,
        maxWidth: 960,
        margin: '0 auto',
        background: branding?.backgroundColor ?? '#f8fafc',
        color: branding?.textColor ?? '#0f172a',
        minHeight: '100vh',
      }}
    >
      <div style={{ marginBottom: 24 }}>
        <div style={{ color: branding?.primaryColor ?? '#2563eb', fontWeight: 800, fontSize: 28 }}>
          {branding?.displayName ?? 'Voyara'}
        </div>
        <div style={{ color: '#64748b', marginTop: 4 }}>{branding?.tagline ?? 'Travel quotation'}</div>
        {theme?.headerHtml ? <div style={{ marginTop: 12 }} dangerouslySetInnerHTML={{ __html: theme.headerHtml }} /> : null}
      </div>

      <span
        style={{
          display: 'inline-block',
          padding: '6px 12px',
          borderRadius: 999,
          background: branding?.accentColor ?? '#dbeafe',
          color: '#111827',
          fontWeight: 700,
        }}
      >
        Public quotation preview
      </span>

      <h1>{quote.title}</h1>
      <p style={{ color: '#475569' }}>
        {quote.customerName} • {quote.destination} • {quote.travellers} traveler(s)
      </p>

      <section style={{ marginTop: 24, border: '1px solid #cbd5e1', borderRadius: 16, padding: 20, background: '#fff' }}>
        <h2 style={{ marginTop: 0 }}>Trip summary</h2>
        <p><strong>Travel dates:</strong> {quote.travelDate} → {quote.returnDate}</p>
        <p><strong>Valid until:</strong> {quote.validUntil}</p>
        <p><strong>Total:</strong> {quote.currency} {quote.totalAmount}</p>
        {quote.visibleNotes ? <p><strong>Notes:</strong> {quote.visibleNotes}</p> : null}
        <QuoteDecisionActions token={params.token} />
      </section>

      <section style={{ marginTop: 24, border: '1px solid #cbd5e1', borderRadius: 16, padding: 20, background: '#fff' }}>
        <h2 style={{ marginTop: 0 }}>Line items</h2>
        <ul style={{ paddingLeft: 20, marginBottom: 0 }}>
          {quote.lineItems.map((item) => (
            <li key={`${item.sortOrder}-${item.description}`} style={{ marginBottom: 8 }}>
              {item.description} — {item.quantity} × {item.unitPriceAmount} {item.currency} = {item.lineTotal} {item.currency}
            </li>
          ))}
        </ul>
      </section>

      {quote.attachments.length > 0 ? (
        <section style={{ marginTop: 24, border: '1px solid #cbd5e1', borderRadius: 16, padding: 20, background: '#fff' }}>
          <h2 style={{ marginTop: 0 }}>Attachments</h2>
          <ul style={{ paddingLeft: 20, marginBottom: 0 }}>
            {quote.attachments.map((attachment) => (
              <li key={attachment.id}>
                <a href={attachment.readUrl} target="_blank" rel="noreferrer" style={{ color: branding?.primaryColor ?? '#2563eb' }}>
                  {attachment.originalFileName}
                </a>
                {attachment.caption ? ` — ${attachment.caption}` : ''}
              </li>
            ))}
          </ul>
        </section>
      ) : null}

      <footer style={{ marginTop: 32, color: '#64748b' }}>
        {theme?.footerHtml ? <div dangerouslySetInnerHTML={{ __html: theme.footerHtml }} /> : null}
        {branding?.supportEmail ? <div>Support: {branding.supportEmail}</div> : null}
        {branding?.supportPhone ? <div>Phone: {branding.supportPhone}</div> : null}
        {branding?.websiteUrl ? <div>Website: {branding.websiteUrl}</div> : null}
      </footer>
    </main>
  );
}
