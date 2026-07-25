import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import { z } from 'zod';
import { useCreateCase, useMe } from './api';
import { bodySystems } from './labels';

const schema = z.object({
  title: z.string().min(2, 'En az 2 karakter'),
  description: z.string().optional(),
  bodySystem: z.string(),
});

type FormValues = z.infer<typeof schema>;

export function NewCasePage() {
  const me = useMe();
  const createCase = useCreateCase(me.data?.id);
  const navigate = useNavigate();
  const { register, handleSubmit, formState: { errors } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { bodySystem: 'Unknown' },
  });

  const onSubmit = (values: FormValues) => {
    if (!me.data) return;
    createCase.mutate(
      { patientId: me.data.id, ...values },
      { onSuccess: (c) => navigate(`/cases/${c.id}`) },
    );
  };

  return (
    <div className="mx-auto max-w-xl">
      <h1 className="text-lg font-semibold text-brand-900">Yeni vaka</h1>
      <p className="mt-1 text-sm text-gray-500">
        Sağlık problemini kısaca anlat; sonraki adımda belgelerini (MR, rapor, tahlil) yükleyeceksin.
      </p>

      <form onSubmit={handleSubmit(onSubmit)} className="mt-6 space-y-4 rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
        <div>
          <label className="text-sm font-medium">Başlık</label>
          <input {...register('title')} placeholder="örn. Baş ağrısı takibi" className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-brand-600 focus:outline-none" />
          {errors.title && <p className="mt-1 text-xs text-red-600">{errors.title.message}</p>}
        </div>
        <div>
          <label className="text-sm font-medium">Ne yaşıyorsun? (opsiyonel)</label>
          <textarea {...register('description')} rows={3} className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-brand-600 focus:outline-none" />
        </div>
        <div>
          <label className="text-sm font-medium">İlgili sistem</label>
          <select {...register('bodySystem')} className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-brand-600 focus:outline-none">
            {bodySystems.map((b) => (
              <option key={b.value} value={b.value}>{b.label}</option>
            ))}
          </select>
        </div>

        {createCase.error && <p className="text-sm text-red-600">{createCase.error.message}</p>}

        <button disabled={createCase.isPending} className="w-full rounded-lg bg-brand-600 py-2 text-sm font-medium text-white hover:bg-brand-700 disabled:opacity-50">
          {createCase.isPending ? 'Oluşturuluyor…' : 'Vakayı oluştur'}
        </button>
      </form>
    </div>
  );
}
