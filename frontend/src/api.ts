import type { UserSession } from './types';

const SESSION_KEY = 'cleantrack-session';

export function getSession(): UserSession | null {
  const raw = localStorage.getItem(SESSION_KEY);
  if (!raw) return null;
  try {
    const session = JSON.parse(raw) as UserSession;
    if (new Date(session.expiresAtUtc).getTime() <= Date.now()) {
      localStorage.removeItem(SESSION_KEY);
      return null;
    }
    return session;
  } catch {
    localStorage.removeItem(SESSION_KEY);
    return null;
  }
}

export function setSession(session: UserSession | null) {
  if (session) localStorage.setItem(SESSION_KEY, JSON.stringify(session));
  else localStorage.removeItem(SESSION_KEY);
}

export async function api<T>(path: string, options: RequestInit = {}): Promise<T> {
  const session = getSession();
  const headers = new Headers(options.headers);
  if (!headers.has('Content-Type') && options.body) headers.set('Content-Type', 'application/json');
  if (session?.token) headers.set('Authorization', `Bearer ${session.token}`);

  const response = await fetch(path, { ...options, headers });
  if (response.status === 401 && path !== '/api/auth/login') {
    setSession(null);
    window.dispatchEvent(new Event('cleantrack-auth-expired'));
  }
  if (!response.ok) {
    let message = `HTTP ${response.status}`;
    try {
      const body = await response.json();
      message = body.message ?? message;
    } catch { /* no-op */ }
    throw new Error(message);
  }
  if (response.status === 204) return undefined as T;
  return response.json() as Promise<T>;
}

export const formatDateTime = (value?: string | null) =>
  value ? new Intl.DateTimeFormat('hu-HU', { dateStyle: 'short', timeStyle: 'short' }).format(new Date(value)) : '—';

export const formatTime = (value?: string | null) =>
  value ? new Intl.DateTimeFormat('hu-HU', { hour: '2-digit', minute: '2-digit' }).format(new Date(value)) : '—';
