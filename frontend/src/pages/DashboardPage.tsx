import { useEffect, useState } from 'react';
import { api, formatTime } from '../api';
import type { DashboardData } from '../types';
import StatCard from '../components/StatCard';

export default function DashboardPage() {
  const [data, setData] = useState<DashboardData | null>(null);
  const [error, setError] = useState('');

  const load = async () => { try { setData(await api<DashboardData>('/api/dashboard/today')); setError(''); } catch (e) { setError(e instanceof Error ? e.message : 'Hiba'); } };
  useEffect(() => { void load(); const timer = window.setInterval(load, 30000); return () => clearInterval(timer); }, []);

  return <section><div className="page-header"><div><p className="eyebrow">Mai állapot</p><h1>Áttekintés</h1><p>Valós idejű jelenléti kép a takarítói csapatról.</p></div><button className="ghost-button" onClick={load}>Frissítés</button></div>{error && <div className="alert error">{error}</div>}{!data ? <div className="panel loading">Adatok betöltése…</div> : <><div className="stats-grid"><StatCard label="Jelenleg bent" value={data.present} hint="aktív dolgozó" tone="good"/><StatCard label="Ma megjelent" value={data.checkedToday} hint={`${data.activeEmployees} aktívból`} /><StatCard label="Mai esemény" value={data.eventsToday} hint="be- és kicsekkolás"/><StatCard label="Még nem jelent meg" value={Math.max(0, data.activeEmployees - data.checkedToday)} hint="mai napon" tone="warn"/></div><div className="panel"><div className="panel-title"><div><h2>Mai jelenlét</h2><p>A lista 30 másodpercenként automatikusan frissül.</p></div></div><div className="table-wrap"><table><thead><tr><th>Dolgozó</th><th>Első érkezés</th><th>Utolsó esemény</th><th>Állapot</th></tr></thead><tbody>{data.rows.length === 0 ? <tr><td colSpan={4} className="empty">Még nincs mai bejegyzés.</td></tr> : data.rows.map(row => <tr key={row.employeeId}><td><strong>{row.employeeName}</strong></td><td>{formatTime(row.firstCheckInUtc)}</td><td>{formatTime(row.lastEventUtc)}</td><td><span className={row.isPresent ? 'status present' : 'status away'}>{row.isPresent ? 'Bent' : 'Kint'}</span></td></tr>)}</tbody></table></div></div></> }</section>;
}
