import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { Link, useNavigate } from 'react-router-dom';
import { z } from 'zod';
import { useRegisterPatient } from './api';

const schema = z.object({
  fullName: z.string().min(2, 'En az 2 karakter'),
  email: z.string().email('Geçerli bir e-posta girin'),
  password: z.string().min(8, 'En az 8 karakter'),
  dateOfBirth: z.string().optional(),
});

type FormValues = z.infer<typeof schema>;

export function RegisterPage() {
  const { register, handleSubmit, formState: { errors } } = useForm<FormValues>({ resolver: zodResolver(schema) });
  const registerPatient = useRegisterPatient();
  const navigate = useNavigate();

  const onSubmit = (values: FormValues) =>
    registerPatient.mutate(
      { ...values, dateOfBirth: values.dateOfBirth || null },
      { onSuccess: () => navigate('/login') },
    );

  return (
    <div className="flex min-h-screen items-center justify-center px-4">
      <div className="w-full max-w-sm rounded-xl border border-gray-200 bg-white p-8 shadow-sm">
        <h1 className="text-xl font-semibold text-brand-900">Hasta kaydı</h1>
        <p className="mt-1 text-sm text-gray-500">Birkaç adımda sağlık yolculuğuna başla</p>

        <form onSubmit={handleSubmit(onSubmit)} className="mt-6 space-y-4">
          <div>
            <label className="text-sm font-medium">Ad Soyad</label>
            <input {...register('fullName')} className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-brand-600 focus:outline-none" />
            {errors.fullName && <p className="mt-1 text-xs text-red-600">{errors.fullName.message}</p>}
          </div>
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
          <div>
            <label className="text-sm font-medium">Doğum tarihi (opsiyonel)</label>
            <input {...register('dateOfBirth')} type="date" className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-brand-600 focus:outline-none" />
          </div>

          {registerPatient.error && <p className="text-sm text-red-600">{registerPatient.error.message}</p>}

          <button disabled={registerPatient.isPending} className="w-full rounded-lg bg-brand-600 py-2 text-sm font-medium text-white hover:bg-brand-700 disabled:opacity-50">
            {registerPatient.isPending ? 'Kaydediliyor…' : 'Kayıt ol'}
          </button>
        </form>

        <p className="mt-4 text-center text-sm text-gray-500">
          Zaten hesabın var mı?{' '}
          <Link to="/login" className="font-medium text-brand-600 hover:underline">Giriş yap</Link>
        </p>
      </div>
    </div>
  );
}
