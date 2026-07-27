import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { DoctorActionsTab } from '../doctor/components/DoctorActionsTab';
import { useAuth } from '../../lib/auth';
import { useCase, useHealthRoute } from './api';
import { AnalysesTab } from './components/AnalysesTab';
import { DoctorsTab } from './components/DoctorsTab';
import { DocumentsTab } from './components/DocumentsTab';
import { MessagesTab } from './components/MessagesTab';
import { RouteTab } from './components/RouteTab';
import { TimelineTab } from './components/TimelineTab';
import { statusLabels } from './labels';

const baseTabs = [
  { key: 'route', label: 'Sağlık Rotası' },
  { key: 'documents', label: 'Belgeler' },
  { key: 'analyses', label: 'Hızır Analizi' },
  { key: 'doctors', label: 'Doktorlar' },
  { key: 'messages', label: 'Mesajlar' },
  { key: 'timeline', label: 'Zaman Çizelgesi' },
] as const;

const doctorTab = { key: 'doctor-actions', label: 'Doktor Aksiyonları' } as const;

type TabKey = (typeof baseTabs)[number]['key'] | typeof doctorTab.key;

export function CaseDetailPage() {
  const { id = '' } = useParams();
  const { role } = useAuth();
  const medicalCase = useCase(id);
  const route = useHealthRoute(id);
  const isDoctor = role === 'Doctor';
  const tabs = isDoctor
    ? ([doctorTab, ...baseTabs.filter((t) => t.key !== 'doctors')] as const)
    : baseTabs;
  const [tab, setTab] = useState<TabKey>(isDoctor ? 'doctor-actions' : 'route');

  if (medicalCase.isLoading) return <p className="text-sm text-gray-500">Yükleniyor…</p>;
  if (!medicalCase.data) return <p className="text-sm text-red-600">Vaka bulunamadı.</p>;

  return (
    <div className="mx-auto max-w-3xl">
      <div className="rounded-2xl border border-gray-200 bg-white p-6 shadow-sm">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <h1 className="text-lg font-semibold text-brand-900">{medicalCase.data.title}</h1>
          <span className="rounded-full bg-brand-50 px-3 py-1 text-xs font-medium text-brand-700">
            {statusLabels[medicalCase.data.status]}
          </span>
        </div>
        {route.data && (
          <p className="mt-2 text-sm text-gray-600">
            <span className="font-medium text-brand-600">Sıradaki adım:</span> {route.data.nextStep}
          </p>
        )}
      </div>

      <div className="mt-4 flex flex-wrap gap-1 border-b border-gray-200">
        {tabs.map((t) => (
          <button
            key={t.key}
            onClick={() => setTab(t.key)}
            className={`rounded-t-lg px-4 py-2 text-sm font-medium ${
              tab === t.key ? 'border-b-2 border-brand-600 text-brand-700' : 'text-gray-500 hover:text-gray-700'
            }`}
          >
            {t.label}
          </button>
        ))}
      </div>

      <div className="mt-4">
        {tab === 'doctor-actions' && <DoctorActionsTab caseId={id} />}
        {tab === 'route' && <RouteTab caseId={id} />}
        {tab === 'documents' && <DocumentsTab caseId={id} />}
        {tab === 'analyses' && <AnalysesTab caseId={id} />}
        {tab === 'doctors' && <DoctorsTab caseId={id} />}
        {tab === 'messages' && <MessagesTab caseId={id} />}
        {tab === 'timeline' && <TimelineTab caseId={id} />}
      </div>
    </div>
  );
}
