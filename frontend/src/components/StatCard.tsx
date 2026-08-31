export default function StatCard({ label, value, hint, tone = 'default' }: { label: string; value: string | number; hint: string; tone?: 'default' | 'good' | 'warn' }) {
  return <div className={`stat-card ${tone}`}><span className="stat-label">{label}</span><strong>{value}</strong><small>{hint}</small></div>;
}
