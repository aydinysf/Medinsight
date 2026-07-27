import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { createBrowserRouter, Navigate, RouterProvider } from 'react-router-dom';
import { AppLayout } from './AppLayout';
import { LoginPage } from './features/auth/LoginPage';
import { RegisterDoctorPage } from './features/auth/RegisterDoctorPage';
import { RegisterPage } from './features/auth/RegisterPage';
import { CaseDetailPage } from './features/cases/CaseDetailPage';
import { CasesPage } from './features/cases/CasesPage';
import { NewCasePage } from './features/cases/NewCasePage';
import { DoctorHomePage } from './features/doctor/DoctorHomePage';
import './index.css';
import { AuthProvider, RequireAuth, useAuth } from './lib/auth';
import { HomePage } from './pages/HomePage';

/** Rol bazlı giriş: doktor panele, diğerleri Hızır karşılamasına. */
function RoleHome() {
  const { role } = useAuth();
  return role === 'Doctor' ? <Navigate to="/doctor" replace /> : <HomePage />;
}

const queryClient = new QueryClient({
  defaultOptions: { queries: { retry: 1, refetchOnWindowFocus: false } },
});

const router = createBrowserRouter([
  { path: '/login', element: <LoginPage /> },
  { path: '/register', element: <RegisterPage /> },
  { path: '/register/doctor', element: <RegisterDoctorPage /> },
  {
    element: (
      <RequireAuth>
        <AppLayout />
      </RequireAuth>
    ),
    children: [
      { path: '/', element: <RoleHome /> },
      { path: '/doctor', element: <DoctorHomePage /> },
      { path: '/cases', element: <CasesPage /> },
      { path: '/cases/new', element: <NewCasePage /> },
      { path: '/cases/:id', element: <CaseDetailPage /> },
    ],
  },
]);

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <RouterProvider router={router} />
      </AuthProvider>
    </QueryClientProvider>
  </StrictMode>,
);
