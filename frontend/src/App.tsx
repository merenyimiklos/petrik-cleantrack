import { useEffect, useState } from 'react';
import { getSession, setSession } from './api';
import type { UserSession } from './types';
import LoginPage from './pages/LoginPage';
import Layout, { type PageKey } from './components/Layout';
import DashboardPage from './pages/DashboardPage';
import EmployeesPage from './pages/EmployeesPage';
import AttendancePage from './pages/AttendancePage';
import TerminalsPage from './pages/TerminalsPage';

export default function App() {
  const [session, updateSession] = useState<UserSession | null>(() => getSession());
  const [page, setPage] = useState<PageKey>('dashboard');

  useEffect(() => {
    const onExpired = () => updateSession(null);
    window.addEventListener('cleantrack-auth-expired', onExpired);
    return () => window.removeEventListener('cleantrack-auth-expired', onExpired);
  }, []);

  if (!session) {
    return <LoginPage onLogin={(next) => { setSession(next); updateSession(next); }} />;
  }

  const content = page === 'dashboard' ? <DashboardPage />
    : page === 'employees' ? <EmployeesPage />
    : page === 'attendance' ? <AttendancePage />
    : <TerminalsPage />;

  return (
    <Layout
      session={session}
      page={page}
      onPageChange={setPage}
      onLogout={() => { setSession(null); updateSession(null); }}
    >
      {content}
    </Layout>
  );
}
