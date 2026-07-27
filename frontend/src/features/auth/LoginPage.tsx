import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { Link, useNavigate } from 'react-router-dom';
import { z } from 'zod';
import { useAuth } from '../../lib/auth';
import { useLogin } from './api';

const schema = z.object({
  email: z.string().email('Geçerli bir e-posta girin'),
  password: z.string().min(1, 'Parola gerekli'),
});

type FormValues = z.infer<typeof schema>;

export function LoginPage() {
  const { register, handleSubmit, formState: { errors } } = useForm<FormValues>({ resolver: zodResolver(schema) });
  const login = useLogin();
  const { signIn } = useAuth();
  const navigate = useNavigate();

  const onSubmit = (values: FormValues) =>
    login.mutate(values, {
      onSuccess: (r) => {
        signIn(r.accessToken, r.userId, r.role);
        navigate('/');
      },
    });

  return (
    <div className="flex min-h-screen items-center justify-center px-4">
      <div className="w-full max-w-sm rounded-xl border border-gray-200 bg-white p-8 shadow-sm">
        <h1 className="text-xl font-semibold text-brand-900">MedInsight</h1>
        <p className="mt-1 text-sm text-gray-500">Sağlık yolculuğunda yol arkadaşın</p>

        <form onSubmit={handleSubmit(onSubmit)} className="mt-6 space-y-4">
          <div>
            <label className="text-sm font-medium">E-posta</label>
            <input {...register('email')} type="email" className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-brand-600 focus:outline-none" />
            {errors.email && <p className="mt-1 text-xs text-red-600">{errors.email.message}</p>}
          </div>
          <div>
            <label className="text-sm font-medium">Parola</label>
            <input {...register('password')} type="password" className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-brand-600 focus:outline-none" />
            {errors.password && <p className="mt-1 text-xs text-red-600">{errors.password.message}</p>}
          </div>

          {login.error && <p className="text-sm text-red-600">Giriş başarısız — e-posta veya parola hatalı.</p>}

          <button disabled={login.isPending} className="w-full rounded-lg bg-brand-600 py-2 text-sm font-medium text-white hover:bg-brand-700 disabled:opacity-50">
            {login.isPending ? 'Giriş yapılıyor…' : 'Giriş yap'}
          </button>
        </form>

        <p className="mt-4 text-center text-sm text-gray-500">
          Hesabın yok mu?{' '}
          <Link to="/register" className="font-medium text-brand-600 hover:underline">Kayıt ol</Link>
        </p>
        <p className="mt-1 text-center text-sm text-gray-500">
          Doktor musunuz?{' '}
          <Link to="/register/doctor" className="font-medium text-brand-600 hover:underline">Doktor kaydı</Link>
        </p>
      </div>
    </div>
  );
}
