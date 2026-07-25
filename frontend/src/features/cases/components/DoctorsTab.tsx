import { useConsultations, useDoctorMatches, useStartConsultation } from '../api';

export function DoctorsTab({ caseId }: { caseId: string }) {
  const matches = useDoctorMatches(caseId);
  const consultations = useConsultations(caseId);
  const startConsultation = useStartConsultation(caseId);

  const activeDoctorIds = new Set(
    consultations.data?.filter((c) => c.status === 'Active').map((c) => c.doctorId) ?? [],
  );

  return (
    <div className="space-y-4">
      <p className="text-sm text-gray-500">
        Vakan için önerilen doktorlar — bu bir atama değil, öneridir; seçim senindir.
      </p>

      {matches.data?.length === 0 && (
        <p className="rounded-xl border border-dashed border-gray-300 bg-white p-6 text-center text-sm text-gray-500">
          Şu an önerilebilecek doğrulanmış doktor yok.
        </p>
      )}

      {matches.data?.map((m) => (
        <div key={m.doctorId} className="rounded-xl border border-gray-200 bg-white p-4 shadow-sm">
          <div className="flex items-center justify-between gap-2">
            <div>
              <p className="font-medium text-brand-900">
                {m.title ? `${m.title} ` : ''}{m.fullName}
                {m.availabilityTag === 'Busy' && (
                  <span className="ml-2 rounded-full bg-amber-50 px-2 py-0.5 text-xs font-medium text-amber-700">yoğun</span>
                )}
              </p>
              <p className="text-sm text-gray-500">{m.specialty}</p>
            </div>
            {activeDoctorIds.has(m.doctorId) ? (
              <span className="rounded-full bg-green-50 px-3 py-1 text-xs font-medium text-green-700">Vakanda</span>
            ) : (
              <button
                onClick={() => startConsultation.mutate(m.doctorId)}
                disabled={startConsultation.isPending}
                className="rounded-lg bg-brand-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-brand-700 disabled:opacity-50"
              >
                Konsültasyon başlat
              </button>
            )}
          </div>
          <details className="mt-2">
            <summary className="cursor-pointer text-xs text-gray-400">Neden önerildi? (skor: {m.score.toFixed(2)})</summary>
            <ul className="mt-1 grid grid-cols-2 gap-x-4 text-xs text-gray-500 sm:grid-cols-3">
              {Object.entries(m.scoreBreakdown).map(([k, v]) => (
                <li key={k}>{k}: {v.toFixed(2)}</li>
              ))}
            </ul>
          </details>
        </div>
      ))}

      {startConsultation.error && <p className="text-sm text-red-600">{startConsultation.error.message}</p>}
    </div>
  );
}
