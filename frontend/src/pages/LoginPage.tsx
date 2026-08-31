import { useState, type FormEvent } from 'react';
import { api } from '../api';
import type { UserSession } from '../types';

export default function LoginPage({ onLogin }: { onLogin: (session: UserSession) => void }) {
  const [email, setEmail] = useState('admin@petrik.hu');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  async function submit(e: FormEvent) {
    e.preventDefault(); setError(''); setLoading(true);
    try {
      const session = await api<UserSession>('/api/auth/login', { method: 'POST', body: JSON.stringify({ email, password }) });
      onLogin(session);
    } catch (e) { setError(e instanceof Error ? e.message : 'Sikertelen belépés.'); }
    finally { setLoading(false); }
  }

  return <div className="login-screen"><div className="login-card"><div className="login-brand"><div className="brand-mark large">CT</div><div><h1>Petrik CleanTrack</h1><p>Munkaidő-nyilvántartó adminisztráció</p></div></div><form onSubmit={submit}><label>E-mail cím<input type="email" value={email} onChange={e => setEmail(e.target.value)} required /></label><label>Jelszó<input type="password" value={password} onChange={e => setPassword(e.target.value)} required autoFocus /></label>{error && <div className="alert error">{error}</div>}<button className="primary-button full" disabled={loading}>{loading ? 'Belépés…' : 'Belépés'}</button></form></div></div>;
}
