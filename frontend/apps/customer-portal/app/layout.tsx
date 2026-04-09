import type { ReactNode } from 'react';
import { CustomerBrandingShell } from './branding-shell';

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="en">
      <body>
        <CustomerBrandingShell>{children}</CustomerBrandingShell>
      </body>
    </html>
  );
}
