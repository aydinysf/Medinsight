import type { Availability, AvailabilityStatus } from '../../../lib/types';
import { useSetAvailability } from '../api';

const options: { value: AvailabilityStatus; label: string }[] = [
  { value: 'Available', label: 'Müsait' },
  { value: 'Busy', label: 'Meşgul' },
  { value: 'Away', label: 'Uzakta' },
];

const statusColors: Record<AvailabilityStatus, string> = {
  Available: 'bg-emerald-100 text-emerald-700',
  Busy: 'bg-amber-100 text-amber-700',
  Away: 'bg-gray-200 text-gray-600',
};

/** EffectiveStatus = ManualOverride ?? ComputedStatus (ADR-009). */
export function AvailabilityToggle({ availability }: { availability: Availability }) {
  const setAvailability = useSetAvailability();

  return (
    <div className="text-right">
      <span className={`rounded-full px-3 py-1 text-xs font-medium ${statusColors[availability.effectiveStatus]}`}>
        {options.find((o) => o.value === availability.effectiveStatus)?.label}
        {availability.manualOverride ? ' (manuel)' : ' (sistem)'}
      </span>
      <div className="mt-2 flex gap-1">
        {options.map((o) => (
          <button
            key={o.value}
            onClick={() => setAvailability.mutate({ override: o.value })}
            disabled={setAvailability.isPending}
            className={`rounded-lg border px-2.5 py-1 text-xs ${
              availability.manualOverride === o.value
                ? 'border-brand-600 bg-brand-50 text-brand-700'
                : 'border-gray-300 text-gray-600 hover:border-brand-400'
            }`}
          >
            {o.label}
          </button>
        ))}
        {availability.manualOverride && (
          <button
            onClick={() => setAvailability.mutate({ override: null })}
            disabled={setAvailability.isPending}
            className="rounded-lg border border-gray-300 px-2.5 py-1 text-xs text-gray-500 hover:border-brand-400"
            title="Manuel seçimi kaldır — sistem hesabına dön"
          >
            Sisteme bırak
          </button>
        )}
      </div>
    </div>
  );
}
