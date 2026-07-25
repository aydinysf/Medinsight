import { useAnalyses, useImageFindings } from '../api';

export function AnalysesTab({ caseId }: { caseId: string }) {
  const analyses = useAnalyses(caseId);
  const imageFindings = useImageFindings(caseId);

  return (
    <div className="space-y-4">
      {analyses.data?.length === 0 && (
        <p className="rounded-xl border border-dashed border-gray-300 bg-white p-6 text-center text-sm text-gray-500">
          Henüz analiz yok — belgelerin kalite kontrolünden geçtiğinde Hızır ön analizini hazırlar.
        </p>
      )}

      {analyses.data?.map((a) => (
        <div key={a.id} className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
          <div className="flex items-start gap-3">
            <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-brand-600 text-sm font-semibold text-white">H</span>
            <div>
              <p className="text-sm text-brand-900">{a.patientMessage}</p>
              <p className="mt-2 text-xs text-gray-400">
                {new Date(a.createdAtUtc).toLocaleString('tr-TR')} · {a.findings.length} bulgu
              </p>
            </div>
          </div>
          {a.findings.length > 0 && (
            <ul className="mt-3 space-y-1 border-t border-gray-100 pt-3">
              {a.findings.map((f) => (
                <li key={f.id} className="text-xs text-gray-600">• {f.description}</li>
              ))}
            </ul>
          )}
          <p className="mt-3 text-xs text-gray-400">
            Bu bir ön değerlendirmedir; tanı ve tedavi kararları doktorunundur.
          </p>
        </div>
      ))}

      {imageFindings.data && imageFindings.data.length > 0 && (
        <div className="rounded-xl border border-amber-200 bg-amber-50 p-5">
          <p className="text-xs font-semibold uppercase tracking-wide text-amber-700">
            ⚠ Deneysel — doğrulanmamış görüntü bulguları
          </p>
          {imageFindings.data.map((f) => (
            <div key={f.id} className="mt-3">
              <p className="text-sm text-gray-800">{f.description}</p>
              <p className="mt-1 text-xs text-amber-700">{f.disclaimer}</p>
              <p className="text-xs text-gray-400">{f.modelName}</p>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
