import { Link, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from './lib/auth';

export function AppLayout() {
  const { signOut, role } = useAuth();
  const navigate = useNavigate();

  return (
    <div className="min-h-screen">
      <header className="border-b border-gray-200 bg-white">
        <div className="mx-auto flex max-w-4xl items-center justify-between px-4 py-3">
          <Link to="/" className="text-lg font-semibold text-brand-600">MedInsight</Link>
          <nav className="flex items-center gap-4 text-sm">
            {role === 'Doctor' ? (
              <Link to="/doctor" className="text-gray-600 hover:text-brand-600">Panelim</Link>
            ) : (
              <Link to="/cases" className="text-gray-600 hover:text-brand-600">Vakalarım</Link>
            )}
            <button
              onClick={() => {
                signOut();
                navigate('/login');
              }}
              className="text-gray-400 hover:text-gray-600"
            >
              Çıkış
            </button>
          </nav>
        </div>
      </header>
      <main className="mx-auto max-w-4xl px-4 py-8">
        <Outlet />
      </main>
    </div>
  );
}
