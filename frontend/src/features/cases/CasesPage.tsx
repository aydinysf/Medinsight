import { Link } from 'react-router-dom';
import { useCases, useMe } from './api';
import { statusLabels } from './labels';

export function CasesPage() {
  const me = useMe();
  const cases = useCases(me.data?.id);

  return (
    <div className="mx-auto max-w-2xl">
      <div className="flex items-center justify-between">
        <h1 className="text-lg font-semibold text-brand-900">Vakalarım</h1>
        <Link to="/cases/new" className="rounded-lg bg-brand-600 px-4 py-2 text-sm font-medium text-white hover:bg-brand-700">
          + Yeni vaka
        </Link>
      </div>

      <div className="mt-4 space-y-3">
        {cases.isLoading && <p className="text-sm text-gray-500">Yükleniyor…</p>}
        {cases.data?.length === 0 && (
          <p className="rounded-xl border border-dashed border-gray-300 bg-white p-6 text-center text-sm text-gray-500">
            Henüz vakan yok. İlk vakanı açarak belgelerini yükleyebilirsin.
          </p>
        )}
        {cases.data?.map((c) => (
          <Link key={c.id} to={`/cases/${c.id}`} className="block rounded-xl border border-gray-200 bg-white p-4 shadow-sm hover:border-brand-600">
            <div className="flex items-center justify-between">
              <p className="font-medium text-brand-900">{c.title}</p>
              <span className="rounded-full bg-brand-50 px-3 py-1 text-xs font-medium text-brand-700">
                {statusLabels[c.status]}
              </span>
            </div>
            <p className="mt-1 text-xs text-gray-400">{new Date(c.createdAtUtc).toLocaleDateString('tr-TR')}</p>
          </Link>
        ))}
      </div>
    </div>
  );
}
