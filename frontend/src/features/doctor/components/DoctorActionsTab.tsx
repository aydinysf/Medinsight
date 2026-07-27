import { useState } from 'react';
import { useAnalyses, useCase, useConsultations } from '../../cases/api';
import {
  useAddClinicalNote,
  useCloseCase,
  useCompleteConsultation,
  useCreateTreatmentPlan,
  useDoctorMe,
  useRequestEscalation,
  useReviewAnalysis,
} from '../api';

/** Doktorun vaka üzerindeki aksiyonları: AI inceleme, klinik not, tedavi planı, eskalasyon, kapatma. */
export function DoctorActionsTab({ caseId }: { caseId: string }) {
  const me = useDoctorMe();
  const medicalCase = useCase(caseId);
  const consultations = useConsultations(caseId);
  const analyses = useAnalyses(caseId);

  const myConsultation = consultations.data?.find(
    (c) => c.doctorId === me.data?.profile.id && c.status === 'Active',
  );
  const consultationId = myConsultation?.id;

  const review = useReviewAnalysis(caseId);
  const addNote = useAddClinicalNote(caseId, consultationId);
  const createPlan = useCreateTreatmentPlan(caseId, consultationId);
  const complete = useCompleteConsultation(caseId, consultationId);
  const escalate = useRequestEscalation(caseId);
  const closeCase = useCloseCase(caseId);

  const [correctionFor, setCorrectionFor] = useState<string | null>(null);
  const [correctionNotes, setCorrectionNotes] = useState('');
  const [note, setNote] = useState('');
  const [planDescription, setPlanDescription] = useState('');
  const [followUpDate, setFollowUpDate] = useState('');

  if (me.isLoading || consultations.isLoading) return <p className="text-sm text-gray-500">Yükleniyor…</p>;

  if (!myConsultation) {
    return (
      <p className="rounded-xl border border-dashed border-gray-300 bg-white p-6 text-center text-sm text-gray-500">
        Bu vakada aktif konsültasyonunuz yok — aksiyonlar aktif konsültasyon gerektirir.
      </p>
    );
  }

  const input = 'mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-brand-600 focus:outline-none';
  const primaryBtn = 'rounded-lg bg-brand-600 px-4 py-2 text-sm font-medium text-white hover:bg-brand-700 disabled:opacity-50';
  const status = medicalCase.data?.status;

  return (
    <div className="space-y-4">
      {/* AI analiz incelemesi */}
      <div className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
        <h3 className="text-sm font-semibold text-brand-900">Hızır analiz incelemesi</h3>
        <p className="mt-1 text-xs text-gray-500">
          Onay/düzeltme kararınız Learning Loop'a girer; düzeltmede not zorunludur.
        </p>
        {analyses.data?.length === 0 && <p className="mt-3 text-sm text-gray-500">İncelenecek analiz yok.</p>}
        {analyses.data?.map((a) => (
          <div key={a.id} className="mt-3 rounded-lg border border-gray-100 bg-gray-50 p-4">
            <p className="text-sm text-gray-800">{a.summary}</p>
            <p className="mt-1 text-xs text-gray-500">
              Güven skoru: <span className="font-semibold">{(a.confidenceScore * 100).toFixed(0)}%</span>
              {' · '}{a.modelVersion} · {new Date(a.createdAtUtc).toLocaleString('tr-TR')}
            </p>
            {a.differentialDiagnoses?.length > 0 && (
              <ul className="mt-2 space-y-1">
                {a.differentialDiagnoses.map((d) => (
                  <li key={d.id} className="text-xs text-gray-600">
                    • {d.name} — {(d.confidenceScore * 100).toFixed(0)}% ({d.riskLevel})
                  </li>
                ))}
              </ul>
            )}

            {a.reviewDecision ? (
              <p className="mt-3 text-xs font-medium text-emerald-700">
                {a.reviewDecision === 'Approved' ? '✓ Onaylandı' : '✎ Düzeltildi'}
                {a.reviewedAtUtc && ` · ${new Date(a.reviewedAtUtc).toLocaleString('tr-TR')}`}
              </p>
            ) : correctionFor === a.id ? (
              <div className="mt-3 space-y-2">
                <textarea
                  value={correctionNotes}
                  onChange={(e) => setCorrectionNotes(e.target.value)}
                  placeholder="Düzeltme notu (zorunlu)"
                  rows={3}
                  className={input}
                />
                <div className="flex gap-2">
                  <button
                    disabled={review.isPending || correctionNotes.trim().length === 0}
                    onClick={() =>
                      review.mutate(
                        { analysisId: a.id, decision: 'Corrected', correctionNotes },
                        { onSuccess: () => { setCorrectionFor(null); setCorrectionNotes(''); } },
                      )
                    }
                    className={primaryBtn}
                  >
                    Düzeltmeyi kaydet
                  </button>
                  <button
                    onClick={() => setCorrectionFor(null)}
                    className="rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-600"
                  >
                    Vazgeç
                  </button>
                </div>
              </div>
            ) : (
              <div className="mt-3 flex gap-2">
                <button
                  disabled={review.isPending}
                  onClick={() => review.mutate({ analysisId: a.id, decision: 'Approved' })}
                  className="rounded-lg bg-emerald-600 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-700 disabled:opacity-50"
                >
                  Onayla
                </button>
                <button
                  disabled={review.isPending}
                  onClick={() => setCorrectionFor(a.id)}
                  className="rounded-lg border border-amber-400 px-4 py-2 text-sm font-medium text-amber-700 hover:bg-amber-50"
                >
                  Düzelt
                </button>
              </div>
            )}
          </div>
        ))}
        {review.error && <p className="mt-2 text-sm text-red-600">{review.error.message}</p>}
      </div>

      {/* Klinik not */}
      <div className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
        <h3 className="text-sm font-semibold text-brand-900">Klinik not</h3>
        <textarea value={note} onChange={(e) => setNote(e.target.value)} rows={3} className={input} placeholder="Hastaya görünmez, meslektaşlarınız için…" />
        <button
          disabled={addNote.isPending || note.trim().length === 0}
          onClick={() => addNote.mutate(note, { onSuccess: () => setNote('') })}
          className={`mt-2 ${primaryBtn}`}
        >
          {addNote.isPending ? 'Kaydediliyor…' : 'Notu kaydet'}
        </button>
        {addNote.isSuccess && <span className="ml-3 text-xs text-emerald-600">Not kaydedildi ✓</span>}
        {addNote.error && <p className="mt-2 text-sm text-red-600">{addNote.error.message}</p>}
      </div>

      {/* Tedavi planı */}
      <div className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
        <h3 className="text-sm font-semibold text-brand-900">Tedavi planı</h3>
        <p className="mt-1 text-xs text-gray-500">
          Plan kaydedilince sağlık rotası anlık görüntüsü alınır ve vaka tedavi sürecine geçer.
        </p>
        <textarea
          value={planDescription}
          onChange={(e) => setPlanDescription(e.target.value)}
          rows={4}
          className={input}
          placeholder="Tedavi adımları, ilaçlar, öneriler…"
        />
        <div className="mt-2">
          <label className="text-xs font-medium text-gray-600">Kontrol tarihi (opsiyonel)</label>
          <input type="date" value={followUpDate} onChange={(e) => setFollowUpDate(e.target.value)} className={input} />
        </div>
        <button
          disabled={createPlan.isPending || planDescription.trim().length < 3}
          onClick={() =>
            createPlan.mutate(
              { description: planDescription, followUpDate: followUpDate || null },
              { onSuccess: () => { setPlanDescription(''); setFollowUpDate(''); } },
            )
          }
          className={`mt-3 ${primaryBtn}`}
        >
          {createPlan.isPending ? 'Kaydediliyor…' : 'Tedavi planını oluştur'}
        </button>
        {createPlan.isSuccess && <span className="ml-3 text-xs text-emerald-600">Plan oluşturuldu ✓</span>}
        {createPlan.error && <p className="mt-2 text-sm text-red-600">{createPlan.error.message}</p>}
      </div>

      {/* Diğer aksiyonlar */}
      <div className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
        <h3 className="text-sm font-semibold text-brand-900">Diğer aksiyonlar</h3>
        <div className="mt-3 flex flex-wrap gap-2">
          <button
            disabled={escalate.isPending || escalate.isSuccess}
            onClick={() => escalate.mutate()}
            className="rounded-lg border border-amber-400 px-4 py-2 text-sm font-medium text-amber-700 hover:bg-amber-50 disabled:opacity-50"
            title="ADR-014: vaka önceliği yükseltilir, ikinci görüş için işaretlenir"
          >
            {escalate.isSuccess ? 'İkinci görüş talep edildi ✓' : 'İkinci görüş iste'}
          </button>
          <button
            disabled={complete.isPending}
            onClick={() => complete.mutate()}
            className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
          >
            Konsültasyonu tamamla
          </button>
          {status === 'FollowUp' && (
            <button
              disabled={closeCase.isPending}
              onClick={() => closeCase.mutate()}
              className="rounded-lg border border-red-300 px-4 py-2 text-sm font-medium text-red-700 hover:bg-red-50 disabled:opacity-50"
            >
              Vakayı kapat
            </button>
          )}
        </div>
        {(escalate.error || complete.error || closeCase.error) && (
          <p className="mt-2 text-sm text-red-600">
            {(escalate.error ?? complete.error ?? closeCase.error)?.message}
          </p>
        )}
      </div>
    </div>
  );
}
