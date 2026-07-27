import { Link } from 'react-router-dom';
import { statusLabels } from '../../cases/labels';
import { useReviewQueue } from '../api';

const consultationLabels: Record<string, string> = {
  Pending: 'Beklemede',
  Active: 'Aktif',
  Completed: 'Tamamlandı',
};

export function QueueList() {
  const queue = useReviewQueue();

  if (queue.isLoading) return <p className="text-sm text-gray-500">Yükleniyor…</p>;

  if (!queue.data || queue.data.length === 0) {
    return (
      <p className="rounded-xl border border-dashed border-gray-300 bg-white p-6 text-center text-sm text-gray-500">
        Kuyruğunuzda vaka yok — hasta sizi seçtiğinde vakalar burada listelenir.
      </p>
    );
  }

  return (
    <div className="space-y-3">
      {queue.data.map((item) => (
        <Link
          key={item.case.id}
          to={`/cases/${item.case.id}`}
          className="block rounded-xl border border-gray-200 bg-white p-4 shadow-sm transition hover:border-brand-400"
        >
          <div className="flex flex-wrap items-center justify-between gap-2">
            <div className="flex items-center gap-2">
              {item.reviewPriority === 'High' && (
                <span className="rounded-full bg-red-100 px-2.5 py-0.5 text-xs font-semibold text-red-700">
                  Öncelikli
                </span>
              )}
              <span className="text-sm font-medium text-brand-900">{item.case.title}</span>
            </div>
            <div className="flex items-center gap-2 text-xs">
              <span className="rounded-full bg-brand-50 px-2.5 py-0.5 font-medium text-brand-700">
                {statusLabels[item.case.status]}
              </span>
              <span className="rounded-full bg-gray-100 px-2.5 py-0.5 text-gray-600">
                Konsültasyon: {consultationLabels[item.consultationStatus]}
              </span>
            </div>
          </div>
          <p className="mt-2 text-xs text-gray-400">
            Başlangıç: {new Date(item.consultationStartedAtUtc).toLocaleString('tr-TR')}
          </p>
        </Link>
      ))}
    </div>
  );
}
