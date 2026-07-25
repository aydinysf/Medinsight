import { useHealthRoute, useSnapshots } from '../api';
import { triggerLabels } from '../labels';

export function RouteTab({ caseId }: { caseId: string }) {
  const route = useHealthRoute(caseId);
  const snapshots = useSnapshots(caseId);

  return (
    <div className="space-y-4">
      {route.data && (
        <div className="rounded-xl border border-brand-100 bg-brand-50 p-5">
          <p className="text-xs font-medium uppercase tracking-wide text-brand-600">Şu an neredesin</p>
          <p className="mt-1 font-medium text-brand-900">{route.data.currentStatus}</p>
          <p className="mt-2 text-sm text-gray-600">
            <span className="font-medium">Sıradaki adım:</span> {route.data.nextStep}
          </p>
        </div>
      )}

      <div className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
        <h3 className="text-sm font-medium text-gray-500">Rota geçmişi</h3>
        <ol className="mt-3 space-y-3">
          {snapshots.data?.map((s) => (
            <li key={s.id} className="flex gap-3 text-sm">
              <span className="mt-0.5 flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-brand-50 text-xs font-semibold text-brand-700">
                v{s.versionNumber}
              </span>
              <div>
                <p className="text-brand-900">{s.status}</p>
                <p className="text-xs text-gray-400">
                  {triggerLabels[s.triggeredBy] ?? s.triggeredBy} · {new Date(s.createdAtUtc).toLocaleString('tr-TR')} · {s.reason}
                </p>
              </div>
            </li>
          ))}
        </ol>
      </div>
    </div>
  );
}
