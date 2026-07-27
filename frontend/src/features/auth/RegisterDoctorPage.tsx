import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { Link, useNavigate } from 'react-router-dom';
import { z } from 'zod';
import { useRegisterDoctor } from '../doctor/api';

const schema = z.object({
  fullName: z.string().min(2, 'En az 2 karakter'),
  email: z.string().email('Geçerli bir e-posta girin'),
  password: z.string().min(8, 'En az 8 karakter'),
  specialty: z.string().min(2, 'Uzmanlık alanı gerekli'),
  licenseNumber: z.string().min(2, 'Diploma/lisans numarası gerekli'),
  title: z.string().optional(),
  yearsOfExperience: z.number().min(0).max(60),
});

type FormValues = z.infer<typeof schema>;

export function RegisterDoctorPage() {
  const { register, handleSubmit, formState: { errors } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { yearsOfExperience: 0 },
  });
  const registerDoctor = useRegisterDoctor();
  const navigate = useNavigate();

  const onSubmit = (values: FormValues) =>
    registerDoctor.mutate(
      { ...values, title: values.title || null },
      { onSuccess: () => navigate('/login') },
    );

  const input = 'mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-brand-600 focus:outline-none';

  return (
    <div className="flex min-h-screen items-center justify-center px-4 py-8">
      <div className="w-full max-w-sm rounded-xl border border-gray-200 bg-white p-8 shadow-sm">
        <h1 className="text-xl font-semibold text-brand-900">Doktor kaydı</h1>
        <p className="mt-1 text-sm text-gray-500">
          Kayıt sonrası diploma/uzmanlık belgenizi yükleyin — admin onayına kadar vakalara erişilemez.
        </p>

        <form onSubmit={handleSubmit(onSubmit)} className="mt-6 space-y-4">
          <div>
            <label className="text-sm font-medium">Ad Soyad</label>
            <input {...register('fullName')} className={input} />
            {errors.fullName && <p className="mt-1 text-xs text-red-600">{errors.fullName.message}</p>}
          </div>
          <div>
            <label className="text-sm font-medium">Unvan (opsiyonel)</label>
            <input {...register('title')} placeholder="Prof. Dr., Uzm. Dr. …" className={input} />
          </div>
          <div>
            <label className="text-sm font-medium">E-posta</label>
            <input {...register('email')} type="email" className={input} />
            {errors.email && <p className="mt-1 text-xs text-red-600">{errors.email.message}</p>}
          </div>
          <div>
            <label className="text-sm font-medium">Parola</label>
            <input {...register('password')} type="password" className={input} />
            {errors.password && <p className="mt-1 text-xs text-red-600">{errors.password.message}</p>}
          </div>
          <div>
            <label className="text-sm font-medium">Uzmanlık alanı</label>
            <input {...register('specialty')} placeholder="Nöroloji, Kardiyoloji …" className={input} />
            {errors.specialty && <p className="mt-1 text-xs text-red-600">{errors.specialty.message}</p>}
          </div>
          <div className="flex gap-3">
            <div className="flex-1">
              <label className="text-sm font-medium">Lisans no</label>
              <input {...register('licenseNumber')} className={input} />
              {errors.licenseNumber && <p className="mt-1 text-xs text-red-600">{errors.licenseNumber.message}</p>}
            </div>
            <div className="w-28">
              <label className="text-sm font-medium">Deneyim (yıl)</label>
              <input {...register('yearsOfExperience', { valueAsNumber: true })} type="number" min={0} max={60} className={input} />
            </div>
          </div>

          {registerDoctor.error && <p className="text-sm text-red-600">{registerDoctor.error.message}</p>}

          <button
            disabled={registerDoctor.isPending}
            className="w-full rounded-lg bg-brand-600 py-2 text-sm font-medium text-white hover:bg-brand-700 disabled:opacity-50"
          >
            {registerDoctor.isPending ? 'Kaydediliyor…' : 'Kayıt ol'}
          </button>
        </form>

        <p className="mt-4 text-center text-sm text-gray-500">
          Hasta mısınız?{' '}
          <Link to="/register" className="font-medium text-brand-600 hover:underline">Hasta kaydı</Link>
          {' · '}
          <Link to="/login" className="font-medium text-brand-600 hover:underline">Giriş yap</Link>
        </p>
      </div>
    </div>
  );
}
