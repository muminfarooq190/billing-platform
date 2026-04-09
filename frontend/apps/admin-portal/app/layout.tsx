import type { ReactNode } from 'react';
import { AdminBrandingShell } from './branding-shell';

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="en">
      <body>
        <AdminBrandingShell>{children}</AdminBrandingShell>
      </body>
    </html>
  );
}
