import { TenantBrandingProvider } from '@voyara/ui';
import { createBrandingClient } from '@voyara/api-client';

const brandingBaseUrl = process.env.IDENTITY_BASE_URL ?? 'http://localhost:8080';
const tenantId = process.env.DEFAULT_TENANT_ID;

export async function AdminBrandingShell({ children }: { children: React.ReactNode }) {
  const client = createBrandingClient(brandingBaseUrl);
  const branding = await client.getBranding(tenantId).catch(() => null);

  return (
    <TenantBrandingProvider brandingBaseUrl={brandingBaseUrl} tenantId={tenantId} mode="admin">
      <header style={{ padding: '20px 32px', borderBottom: '1px solid #1f2937', background: '#111827' }}>
        <div style={{ maxWidth: 1200, margin: '0 auto', display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 16 }}>
          <div>
            <div style={{ fontSize: 24, fontWeight: 700, color: 'var(--tenant-color-accent)' }}>{branding?.displayName ?? 'Voyara Admin'}</div>
            <div style={{ color: '#94a3b8', fontSize: 14 }}>{branding?.supportEmail ?? 'Tenant administration'}</div>
          </div>
          <div style={{ color: '#94a3b8', fontSize: 12 }}>Theme: {branding?.themeMode ?? 'Light'}</div>
        </div>
      </header>
      {children}
    </TenantBrandingProvider>
  );
}
