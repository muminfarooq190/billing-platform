import { TenantBrandingProvider } from '@voyara/ui';
import { createBrandingClient } from '@voyara/api-client';

const brandingBaseUrl = process.env.IDENTITY_BASE_URL ?? 'http://localhost:8080';
const tenantId = process.env.DEFAULT_TENANT_ID;

export async function CustomerBrandingShell({ children }: { children: React.ReactNode }) {
  const client = createBrandingClient(brandingBaseUrl);
  const branding = await client.getBranding(tenantId).catch(() => null);
  const assets = await client.getAssets(tenantId).catch(() => []);
  const logo = assets.find((x) => x.assetType === 'LogoPrimary' && x.isActive);

  return (
    <TenantBrandingProvider brandingBaseUrl={brandingBaseUrl} tenantId={tenantId} mode="customer">
      <header style={{ padding: '20px 32px', borderBottom: '1px solid #e2e8f0', background: '#ffffff' }}>
        <div style={{ maxWidth: 1100, margin: '0 auto', display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 16 }}>
          <div>
            <div style={{ fontSize: 24, fontWeight: 700, color: 'var(--tenant-color-primary)' }}>{branding?.displayName ?? 'Voyara'}</div>
            <div style={{ color: '#64748b', fontSize: 14 }}>{branding?.tagline ?? 'Customer travel workspace'}</div>
          </div>
          {logo ? <span style={{ color: '#64748b', fontSize: 12 }}>Logo asset: {logo.originalFileName}</span> : null}
        </div>
      </header>
      {children}
    </TenantBrandingProvider>
  );
}
