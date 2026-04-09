import type { ReactNode } from 'react';

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="en">
      <body style={{ fontFamily: 'Arial, sans-serif', margin: 0, background: '#0b1020', color: '#f8fafc' }}>
        {children}
      </body>
    </html>
  );
}
