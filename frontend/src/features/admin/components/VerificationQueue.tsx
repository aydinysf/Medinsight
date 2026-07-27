import { useState } from 'react';
import {
  openVerificationDocument,
  useApproveVerification,
  usePendingVerifications,
  useRejectVerification,
} from '../api';

const documentTypeLabels: Record<string, string> = {
  Diploma: 'Diploma',
  SpecialtyCertificate: 'Uzmanlık belgesi',
  TTBRegistry: 'TTB kayıt belgesi',
};

/** ADR-007: doktor doğrulaması yalnızca admin onayıyla — QR verisi öneridir, karar admin'indir. */
export function VerificationQueue() {
  const pending = usePendingVerifications();
  const approve = useApproveVerification();
  const reject = useRejectVerification();
  const [rejectFor, setRejectFor] = useState<string | null>(null);
  const [reason, setReason] = useState('');

  if (pending.isLoading) return <p className="text-sm text-gray-500">Yükleniyor…</p>;

  if (!pending.data || pending.data.length === 0) {
    return (
      <p className="rounded-xl border border-dashed border-gray-300 bg-white p-6 text-center text-sm text-gray-500">
        Bekleyen doktor doğrulaması yok.
      </p>
    );
  }

  return (
    <div className="space-y-3">
      {pending.data.map((v) => (
        <div key={v.verificationId} className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <p className="text-sm font-semibold text-brand-900">{v.doctorFullName}</p>
              <p className="mt-0.5 text-xs text-gray-500">
                {v.specialty} · Lisans: {v.licenseNumber}
              </p>
              <p className="mt-0.5 text-xs text-gray-400">
                {documentTypeLabels[v.documentType] ?? v.documentType} ·{' '}
                {new Date(v.submittedAtUtc).toLocaleString('tr-TR')}
              </p>
            </div>
            <button
              onClick={() => void openVerificationDocument(v.verificationId)}
              className="rounded-lg border border-gray-300 px-3 py-1.5 text-xs font-medium text-gray-700 hover:border-brand-400"
            >
              Belgeyi görüntüle ↗
            </button>
          </div>

          {v.qrParsedData && (
            <div className="mt-3 rounded-lg bg-gray-50 p-3">
              <p className="text-xs font-medium text-gray-600">QR çözümü (öneri — karar sizindir):</p>
              <pre className="mt-1 overflow-x-auto text-xs text-gray-500">{v.qrParsedData}</pre>
            </div>
          )}

          {rejectFor === v.verificationId ? (
            <div className="mt-3 space-y-2">
              <textarea
                value={reason}
                onChange={(e) => setReason(e.target.value)}
                placeholder="Ret gerekçesi (doktora gösterilir, zorunlu)"
                rows={2}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-brand-600 focus:outline-none"
              />
              <div className="flex gap-2">
                <button
                  disabled={reject.isPending || reason.trim().length < 3}
                  onClick={() =>
                    reject.mutate(
                      { verificationId: v.verificationId, reason },
                      { onSuccess: () => { setRejectFor(null); setReason(''); } },
                    )
                  }
                  className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-50"
                >
                  Reddet
                </button>
                <button
                  onClick={() => setRejectFor(null)}
                  className="rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-600"
                >
                  Vazgeç
                </button>
              </div>
            </div>
          ) : (
            <div className="mt-3 flex gap-2">
              <button
                disabled={approve.isPending}
                onClick={() => approve.mutate(v.verificationId)}
                className="rounded-lg bg-emerald-600 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-700 disabled:opacity-50"
              >
                Onayla
              </button>
              <button
                disabled={reject.isPending}
                onClick={() => setRejectFor(v.verificationId)}
                className="rounded-lg border border-red-300 px-4 py-2 text-sm font-medium text-red-700 hover:bg-red-50"
              >
                Reddet…
              </button>
            </div>
          )}
        </div>
      ))}
      {(approve.error || reject.error) && (
        <p className="text-sm text-red-600">{(approve.error ?? reject.error)?.message}</p>
      )}
    </div>
  );
}
