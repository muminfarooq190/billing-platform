const sections = ['Overview', 'Quotations', 'Trips', 'Documents', 'Billing', 'Notifications', 'Preferences'];

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

export default async function CustomerPortalHomePage() {
  const branding = await getBranding();

  return (
    <main style={{ padding: 32, maxWidth: 1100, margin: '0 auto' }}>
      <h1 style={{ marginTop: 0, color: branding?.primaryColor ?? '#2563eb' }}>{branding?.displayName ?? 'Voyara Customer Portal'}</h1>
      <p style={{ color: '#475569' }}>{branding?.tagline ?? 'Starter shell scaffold for the end-user web experience.'}</p>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: 16, marginTop: 24 }}>
        {sections.map((section) => (
          <section key={section} style={{ border: `1px solid ${branding?.secondaryColor ?? '#cbd5e1'}`, borderRadius: 12, padding: 16, background: '#ffffff' }}>
            <h2 style={{ fontSize: 18, marginTop: 0, color: branding?.primaryColor ?? '#0f172a' }}>{section}</h2>
            <p style={{ marginBottom: 0, color: '#475569' }}>Module placeholder ready for customer-facing implementation.</p>
          </section>
        ))}
      </div>
    </main>
  );
}
