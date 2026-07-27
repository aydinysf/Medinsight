import { useState } from 'react';
import { AuditLogPanel } from './components/AuditLogPanel';
import { VerificationQueue } from './components/VerificationQueue';

const tabs = [
  { key: 'verifications', label: 'Doktor Doğrulamaları' },
  { key: 'audit', label: 'Audit Log' },
] as const;

type TabKey = (typeof tabs)[number]['key'];

export function AdminHomePage() {
  const [tab, setTab] = useState<TabKey>('verifications');

  return (
    <div>
      <h1 className="text-lg font-semibold text-brand-900">Yönetim Paneli</h1>

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
        {tab === 'verifications' && <VerificationQueue />}
        {tab === 'audit' && <AuditLogPanel />}
      </div>
    </div>
  );
}
