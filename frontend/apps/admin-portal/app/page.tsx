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

export default function AdminHomePage() {
  return (
    <main style={{ padding: 32 }}>
      <h1 style={{ marginTop: 0 }}>Voyara Admin Portal</h1>
      <p>Starter shell scaffold for the internal operations platform.</p>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: 16, marginTop: 24 }}>
        {sections.map((section) => (
          <section key={section} style={{ border: '1px solid #334155', borderRadius: 12, padding: 16, background: '#111827' }}>
            <h2 style={{ fontSize: 18, marginTop: 0 }}>{section}</h2>
            <p style={{ marginBottom: 0, color: '#cbd5e1' }}>Module placeholder ready for feature implementation.</p>
          </section>
        ))}
      </div>
    </main>
  );
}
