const sections = [
  'Dashboard',
  'CRM / Contacts',
  'Follow-ups',
  'Quotations',
  'Itineraries',
  'Bookings',
  'Billing',
  'Communications',
  'Webhooks',
  'Identity & Settings',
];

const identityBaseUrl = process.env.IDENTITY_BASE_URL ?? 'http://localhost:8080';
const tenantId = process.env.DEFAULT_TENANT_ID;

async function getBranding() {
  const response = await fetch(`${identityBaseUrl}/tenant-branding`, {
    headers: tenantId ? { 'X-Tenant-Id': tenantId } : {},
    cache: 'no-store',
  });

  if (!response.ok) return null;
  return response.json();
}

export default async function AdminHomePage() {
  const branding = await getBranding();

  return (
    <main style={{ padding: 32, maxWidth: 1200, margin: '0 auto' }}>
      <h1 style={{ marginTop: 0, color: branding?.accentColor ?? '#f59e0b' }}>{branding?.displayName ?? 'Voyara Admin Portal'}</h1>
      <p style={{ color: '#cbd5e1' }}>{branding?.supportEmail ?? 'Starter shell scaffold for the internal operations platform.'}</p>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: 16, marginTop: 24 }}>
        {sections.map((section) => (
          <section key={section} style={{ border: `1px solid ${branding?.secondaryColor ?? '#334155'}`, borderRadius: 12, padding: 16, background: '#111827' }}>
            <h2 style={{ fontSize: 18, marginTop: 0, color: '#f8fafc' }}>{section}</h2>
            <p style={{ marginBottom: 0, color: '#cbd5e1' }}>Module placeholder ready for feature implementation.</p>
          </section>
        ))}
      </div>
    </main>
  );
}
