type PageProps = {
  params: {
    token: string;
  };
};

export default function PublicQuotePage({ params }: PageProps) {
  return (
    <main style={{ padding: 32, maxWidth: 920, margin: '0 auto' }}>
      <span style={{ display: 'inline-block', padding: '6px 12px', borderRadius: 999, background: '#dbeafe', color: '#1d4ed8', fontWeight: 700 }}>
        Public quotation preview
      </span>
      <h1>Quotation token: {params.token}</h1>
      <p>This route is scaffolded for the tokenized public quotation experience backed by the travel service.</p>
      <section style={{ marginTop: 24, border: '1px solid #cbd5e1', borderRadius: 16, padding: 20, background: '#fff' }}>
        <h2 style={{ marginTop: 0 }}>Planned content</h2>
        <ul>
          <li>Trip summary</li>
          <li>Travel dates and destination</li>
          <li>Revision snapshot</li>
          <li>Line-item breakdown</li>
          <li>Visible notes</li>
          <li>Customer-visible attachments</li>
          <li>Expiry state</li>
        </ul>
      </section>
    </main>
  );
}
