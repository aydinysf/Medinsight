import { useTimeline } from '../api';

export function TimelineTab({ caseId }: { caseId: string }) {
  const timeline = useTimeline(caseId);
  const entries = [...(timeline.data ?? [])].sort(
    (a, b) => new Date(b.occurredAt).getTime() - new Date(a.occurredAt).getTime(),
  );

  return (
    <div className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
      <ol className="space-y-4">
        {entries.map((e) => (
          <li key={e.id} className="relative border-l-2 border-brand-100 pl-4">
            <span className="absolute -left-[5px] top-1.5 h-2 w-2 rounded-full bg-brand-600" />
            <p className="text-sm text-brand-900">{e.summary}</p>
            <p className="text-xs text-gray-400">{new Date(e.occurredAt).toLocaleString('tr-TR')}</p>
          </li>
        ))}
        {entries.length === 0 && <p className="text-center text-sm text-gray-500">Henüz olay yok.</p>}
      </ol>
    </div>
  );
}
