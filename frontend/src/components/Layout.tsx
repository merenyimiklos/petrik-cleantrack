import type { ReactNode } from 'react';
import type { UserSession } from '../types';

export type PageKey = 'dashboard' | 'employees' | 'attendance' | 'terminals';

const items: { key: PageKey; label: string; icon: string }[] = [
  { key: 'dashboard', label: 'Áttekintés', icon: '▦' },
  { key: 'employees', label: 'Dolgozók', icon: '♙' },
  { key: 'attendance', label: 'Jelenléti napló', icon: '◷' },
  { key: 'terminals', label: 'Terminálok', icon: '▣' }
];

export default function Layout({ session, page, onPageChange, onLogout, children }: {
  session: UserSession;
  page: PageKey;
  onPageChange: (page: PageKey) => void;
  onLogout: () => void;
  children: ReactNode;
}) {
  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <div className="brand-mark">CT</div>
          <div><strong>CleanTrack</strong><span>Petrik munkaidő</span></div>
        </div>
        <nav>
          {items.map(item => (
            <button key={item.key} className={page === item.key ? 'nav-item active' : 'nav-item'} onClick={() => onPageChange(item.key)}>
              <span className="nav-icon">{item.icon}</span>{item.label}
            </button>
          ))}
        </nav>
        <div className="sidebar-footer">
          <div className="user-card"><div className="avatar">{session.fullName.slice(0, 1).toUpperCase()}</div><div><strong>{session.fullName}</strong><span>{session.role}</span></div></div>
          <button className="ghost-button full" onClick={onLogout}>Kijelentkezés</button>
        </div>
      </aside>
      <main className="main-content">{children}</main>
    </div>
  );
}
