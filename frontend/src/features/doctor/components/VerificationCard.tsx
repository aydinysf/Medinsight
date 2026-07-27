import { useRef, useState } from 'react';
import type { DoctorMe } from '../../../lib/types';
import { useSubmitVerification } from '../api';

const documentTypes = [
  { value: 'Diploma', label: 'Diploma' },
  { value: 'SpecialtyCertificate', label: 'Uzmanlık belgesi' },
  { value: 'TTBRegistry', label: 'TTB kayıt belgesi' },
];

/** ADR-007: belge yüklenir, admin onayı zorunludur — otomatik onay yolu yoktur. */
export function VerificationCard({ me }: { me: DoctorMe }) {
  const submit = useSubmitVerification();
  const fileRef = useRef<HTMLInputElement>(null);
  const [documentType, setDocumentType] = useState(documentTypes[0].value);
  const [qrPayload, setQrPayload] = useState('');

  const pending = me.verifications.find((v) => v.status === 'Pending');
  const rejected = me.verifications.find((v) => v.status === 'Rejected');

  const onSubmit = () => {
    const file = fileRef.current?.files?.[0];
    if (!file) return;
    submit.mutate({ file, documentType, qrPayload: qrPayload || undefined });
  };

  if (pending) {
    return (
      <div className="rounded-xl border border-amber-200 bg-amber-50 p-5">
        <p className="text-sm font-medium text-amber-800">Doğrulama belgeniz admin onayında</p>
        <p className="mt-1 text-xs text-amber-700">
          Belgeniz {new Date(pending.createdAtUtc).toLocaleString('tr-TR')} tarihinde alındı.
          Onaylanana kadar vakalara erişemezsiniz (ADR-007).
        </p>
      </div>
    );
  }

  return (
    <div className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
      <p className="text-sm font-semibold text-brand-900">Hesap doğrulaması gerekli</p>
      {rejected ? (
        <p className="mt-1 text-xs text-red-600">
          Önceki başvurunuz reddedildi{rejected.rejectionReason ? `: ${rejected.rejectionReason}` : '.'} Yeni belge yükleyin.
        </p>
      ) : (
        <p className="mt-1 text-xs text-gray-500">
          Diploma veya uzmanlık belgenizi yükleyin — admin onayından sonra vaka alabilirsiniz.
        </p>
      )}

      <div className="mt-4 space-y-3">
        <div>
          <label className="text-sm font-medium">Belge türü</label>
          <select
            value={documentType}
            onChange={(e) => setDocumentType(e.target.value)}
            className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-brand-600 focus:outline-none"
          >
            {documentTypes.map((t) => (
              <option key={t.value} value={t.value}>{t.label}</option>
            ))}
          </select>
        </div>
        <div>
          <label className="text-sm font-medium">Belge dosyası (PDF/JPG/PNG)</label>
          <input ref={fileRef} type="file" accept=".pdf,.jpg,.jpeg,.png" className="mt-1 w-full text-sm" />
        </div>
        <div>
          <label className="text-sm font-medium">QR içeriği (opsiyonel)</label>
          <input
            value={qrPayload}
            onChange={(e) => setQrPayload(e.target.value)}
            placeholder="e-Devlet belge doğrulama kodu vb."
            className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-brand-600 focus:outline-none"
          />
        </div>

        {submit.error && <p className="text-sm text-red-600">{submit.error.message}</p>}

        <button
          onClick={onSubmit}
          disabled={submit.isPending}
          className="rounded-lg bg-brand-600 px-4 py-2 text-sm font-medium text-white hover:bg-brand-700 disabled:opacity-50"
        >
          {submit.isPending ? 'Yükleniyor…' : 'Belgeyi gönder'}
        </button>
      </div>
    </div>
  );
}
