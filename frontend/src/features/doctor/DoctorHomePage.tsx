import { useDoctorMe } from './api';
import { AvailabilityToggle } from './components/AvailabilityToggle';
import { QueueList } from './components/QueueList';
import { VerificationCard } from './components/VerificationCard';

export function DoctorHomePage() {
  const me = useDoctorMe();

  if (me.isLoading) return <p className="text-sm text-gray-500">Yükleniyor…</p>;
  if (!me.data) return <p className="text-sm text-red-600">Doktor profili bulunamadı.</p>;

  const { profile, availability } = me.data;
  const verified = profile.verificationStatus === 'Verified';

  return (
    <div className="space-y-6">
      <div className="rounded-2xl border border-gray-200 bg-white p-6 shadow-sm">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <h1 className="text-lg font-semibold text-brand-900">
              {profile.title ? `${profile.title} ` : ''}{profile.fullName}
            </h1>
            <p className="mt-1 text-sm text-gray-500">
              {profile.specialty} · {profile.yearsOfExperience} yıl deneyim
            </p>
            <p className="mt-1 text-xs text-gray-400">
              Aktif vaka: {availability.activeCaseCount} / {availability.capacityThreshold}
            </p>
          </div>
          {verified && <AvailabilityToggle availability={availability} />}
        </div>
      </div>

      {!verified && <VerificationCard me={me.data} />}

      {verified && (
        <div>
          <h2 className="text-base font-semibold text-brand-900">İnceleme kuyruğu</h2>
          <p className="mt-1 text-xs text-gray-500">
            Öncelikli vakalar (Hızır güven eşiği altı / ikinci görüş talepleri) üstte listelenir.
          </p>
          <div className="mt-3">
            <QueueList />
          </div>
        </div>
      )}
    </div>
  );
}
