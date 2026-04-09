import type { ReactNode } from 'react';

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="en">
      <body style={{ fontFamily: 'Arial, sans-serif', margin: 0, background: '#f8fafc', color: '#0f172a' }}>
        {children}
      </body>
    </html>
  );
}
