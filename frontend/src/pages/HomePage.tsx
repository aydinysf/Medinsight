import { Link } from 'react-router-dom';
import { useCases, useMe } from '../features/cases/api';
import { statusLabels } from '../features/cases/labels';

export function HomePage() {
  const me = useMe();
  const cases = useCases(me.data?.id);
  const activeCase = cases.data?.find((c) => c.status !== 'Closed');
  const firstName = me.data?.fullName.split(' ')[0] ?? '';

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <section className="rounded-2xl border border-brand-100 bg-white p-8 shadow-sm">
        <p className="text-2xl font-semibold text-brand-900">Merhaba {firstName}</p>
        <p className="mt-2 text-lg text-brand-600">Ben Hızır.</p>
        <p className="text-gray-600">Bugün sana nasıl yardımcı olabilirim?</p>

        <div className="mt-6 flex flex-wrap gap-3">
          <Link to="/cases/new" className="rounded-lg bg-brand-600 px-4 py-2 text-sm font-medium text-white hover:bg-brand-700">
            📎 Yeni vaka aç ve belge yükle
          </Link>
          <Link to="/cases" className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50">
            Vakalarım
          </Link>
        </div>

        <p className="mt-6 text-xs text-gray-400">
          MedInsight bir klinik karar destek sistemidir; tanı koymaz, doktorunun yerini almaz.
        </p>
      </section>

      {activeCase && (
        <section className="rounded-2xl border border-gray-200 bg-white p-6 shadow-sm">
          <h2 className="text-sm font-medium text-gray-500">Devam eden vakan</h2>
          <div className="mt-2 flex items-center justify-between">
            <div>
              <p className="font-medium text-brand-900">{activeCase.title}</p>
              <p className="text-sm text-gray-500">{statusLabels[activeCase.status]}</p>
            </div>
            <Link to={`/cases/${activeCase.id}`} className="rounded-lg bg-brand-50 px-4 py-2 text-sm font-medium text-brand-700 hover:bg-brand-100">
              Devam et →
            </Link>
          </div>
        </section>
      )}
    </div>
  );
}
