import { useState } from 'react';
import { useAuditLogs } from '../api';

/** Append-only audit kayıtları — KVKK denetim görünümü (audit-service.md). */
export function AuditLogPanel() {
  const [entityId, setEntityId] = useState('');
  const [expanded, setExpanded] = useState<string | null>(null);
  const logs = useAuditLogs(entityId.trim());

  return (
    <div>
      <div className="flex flex-wrap items-center gap-2">
        <input
          value={entityId}
          onChange={(e) => setEntityId(e.target.value)}
          placeholder="Entity Id ile filtrele (örn. vaka GUID'i)"
          className="w-80 rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-brand-600 focus:outline-none"
        />
        <span className="text-xs text-gray-400">Son 50 kayıt, 15 sn'de bir yenilenir.</span>
      </div>

      {logs.isLoading && <p className="mt-4 text-sm text-gray-500">Yükleniyor…</p>}
      {logs.error && <p className="mt-4 text-sm text-red-600">{logs.error.message}</p>}

      <div className="mt-4 overflow-x-auto rounded-xl border border-gray-200 bg-white shadow-sm">
        <table className="min-w-full text-left text-sm">
          <thead className="border-b border-gray-200 bg-gray-50 text-xs uppercase tracking-wide text-gray-500">
            <tr>
              <th className="px-4 py-2">Zaman</th>
              <th className="px-4 py-2">Aksiyon</th>
              <th className="px-4 py-2">Entity</th>
              <th className="px-4 py-2">Aktör</th>
              <th className="px-4 py-2"></th>
            </tr>
          </thead>
          <tbody>
            {logs.data?.map((l) => (
              <>
                <tr key={l.id} className="border-b border-gray-100 last:border-0">
                  <td className="whitespace-nowrap px-4 py-2 text-xs text-gray-500">
                    {new Date(l.occurredAtUtc).toLocaleString('tr-TR')}
                  </td>
                  <td className="px-4 py-2 font-medium text-brand-900">{l.action}</td>
                  <td className="px-4 py-2 text-xs text-gray-500">
                    {l.entityType}
                    {l.entityId && <span className="block text-gray-400">{l.entityId.slice(0, 8)}…</span>}
                  </td>
                  <td className="px-4 py-2 text-xs text-gray-500">{l.actorId ? `${l.actorId.slice(0, 8)}…` : 'Sistem'}</td>
                  <td className="px-4 py-2">
                    <button
                      onClick={() => setExpanded(expanded === l.id ? null : l.id)}
                      className="text-xs text-brand-600 hover:underline"
                    >
                      {expanded === l.id ? 'Gizle' : 'Detay'}
                    </button>
                  </td>
                </tr>
                {expanded === l.id && (
                  <tr key={`${l.id}-detail`} className="border-b border-gray-100 bg-gray-50">
                    <td colSpan={5} className="px-4 py-2">
                      <pre className="overflow-x-auto text-xs text-gray-600">
                        {JSON.stringify(JSON.parse(l.metadataJson), null, 2)}
                      </pre>
                      <p className="mt-1 text-xs text-gray-400">correlationId: {l.correlationId}</p>
                    </td>
                  </tr>
                )}
              </>
            ))}
            {logs.data?.length === 0 && (
              <tr>
                <td colSpan={5} className="px-4 py-6 text-center text-sm text-gray-500">Kayıt bulunamadı.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
