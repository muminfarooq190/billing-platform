'use client';

import { useState } from 'react';

const travelBaseUrl = process.env.NEXT_PUBLIC_TRAVEL_BASE_URL ?? 'http://localhost:8082';

export function QuoteDecisionActions({ token }: { token: string }) {
  const [status, setStatus] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function submit(action: 'accept' | 'reject') {
    setBusy(true);
    setStatus(null);

    const response = await fetch(`${travelBaseUrl}/travel/quotations/public/${token}/${action}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ reason: action === 'accept' ? 'Accepted by customer' : 'Rejected by customer' }),
    });

    setBusy(false);
    setStatus(response.ok ? (action === 'accept' ? 'Quotation accepted.' : 'Quotation rejected.') : 'Action failed.');
  }

  return (
    <div style={{ display: 'flex', gap: 12, alignItems: 'center', marginTop: 20 }}>
      <button onClick={() => submit('accept')} disabled={busy} style={{ padding: '10px 18px', borderRadius: 10, border: 'none', background: '#16a34a', color: '#fff', fontWeight: 700 }}>
        Accept Quote
      </button>
      <button onClick={() => submit('reject')} disabled={busy} style={{ padding: '10px 18px', borderRadius: 10, border: '1px solid #dc2626', background: '#fff', color: '#dc2626', fontWeight: 700 }}>
        Reject Quote
      </button>
      {status ? <span style={{ color: '#475569' }}>{status}</span> : null}
    </div>
  );
}
