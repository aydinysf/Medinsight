import { useRef } from 'react';
import { useDocuments, useUploadDocuments } from '../api';

const documentStatusLabels: Record<string, string> = {
  Uploaded: 'Yüklendi',
  Classified: 'Sınıflandırıldı',
  QualityChecked: 'Kalite kontrolünden geçti',
  Rejected: 'Reddedildi',
  ClassificationFailed: 'Tanınamadı — tekrar yükleyin',
};

export function DocumentsTab({ caseId }: { caseId: string }) {
  const documents = useDocuments(caseId);
  const upload = useUploadDocuments(caseId);
  const inputRef = useRef<HTMLInputElement>(null);

  const onFilesSelected = (files: FileList | null) => {
    if (files?.length) upload.mutate([...files]);
    if (inputRef.current) inputRef.current.value = '';
  };

  return (
    <div className="space-y-4">
      <div className="rounded-xl border-2 border-dashed border-brand-100 bg-white p-6 text-center">
        <p className="text-sm text-gray-600">MR, BT, rapor (PDF), tahlil veya fotoğraf yükle — toplu seçim yapabilirsin.</p>
        <input ref={inputRef} type="file" multiple hidden onChange={(e) => onFilesSelected(e.target.files)} />
        <button
          onClick={() => inputRef.current?.click()}
          disabled={upload.isPending}
          className="mt-3 rounded-lg bg-brand-600 px-4 py-2 text-sm font-medium text-white hover:bg-brand-700 disabled:opacity-50"
        >
          {upload.isPending ? 'Yükleniyor…' : '📎 Belge seç ve yükle'}
        </button>
        {upload.error && <p className="mt-2 text-sm text-red-600">{upload.error.message}</p>}
      </div>

      <div className="space-y-2">
        {documents.data?.map((d) => (
          <div key={d.id} className="flex items-center justify-between rounded-xl border border-gray-200 bg-white px-4 py-3 shadow-sm">
            <div>
              <p className="text-sm font-medium text-brand-900">{d.originalFileName ?? d.title}</p>
              <p className="text-xs text-gray-400">{(d.sizeBytes / 1024).toFixed(1)} KB · {d.type}</p>
            </div>
            <span
              className={`rounded-full px-3 py-1 text-xs font-medium ${
                d.status === 'QualityChecked'
                  ? 'bg-green-50 text-green-700'
                  : d.status === 'Rejected' || d.status === 'ClassificationFailed'
                    ? 'bg-red-50 text-red-700'
                    : 'bg-gray-100 text-gray-600'
              }`}
            >
              {documentStatusLabels[d.status] ?? d.status}
            </span>
          </div>
        ))}
        {documents.data?.length === 0 && <p className="text-center text-sm text-gray-500">Henüz belge yok.</p>}
      </div>
    </div>
  );
}
