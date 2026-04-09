const sections = ['Overview', 'Quotations', 'Trips', 'Documents', 'Billing', 'Notifications', 'Preferences'];

export default function CustomerPortalHomePage() {
  return (
    <main style={{ padding: 32, maxWidth: 1100, margin: '0 auto' }}>
      <h1 style={{ marginTop: 0 }}>Voyara Customer Portal</h1>
      <p>Starter shell scaffold for the end-user web experience.</p>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: 16, marginTop: 24 }}>
        {sections.map((section) => (
          <section key={section} style={{ border: '1px solid #cbd5e1', borderRadius: 12, padding: 16, background: '#ffffff' }}>
            <h2 style={{ fontSize: 18, marginTop: 0 }}>{section}</h2>
            <p style={{ marginBottom: 0, color: '#475569' }}>Module placeholder ready for customer-facing implementation.</p>
          </section>
        ))}
      </div>
    </main>
  );
}
