import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '../../lib/api';
import type {
  Availability,
  AvailabilityStatus,
  Case,
  Consultation,
  DoctorMe,
  DoctorProfile,
  DoctorQueueItem,
  Treatment,
  Verification,
} from '../../lib/types';
import { caseKeys } from '../cases/api';

const KEYS = {
  me: ['doctors', 'me'] as const,
  queue: ['doctors', 'me', 'cases'] as const,
};

export const useDoctorMe = () =>
  useQuery({ queryKey: KEYS.me, queryFn: () => api.get<DoctorMe, DoctorMe>('/doctors/me') });

export const useReviewQueue = () =>
  useQuery({
    queryKey: KEYS.queue,
    queryFn: () => api.get<DoctorQueueItem[], DoctorQueueItem[]>('/doctors/me/cases'),
    refetchInterval: 10000,
  });

export interface RegisterDoctorDto {
  fullName: string;
  email: string;
  password: string;
  specialty: string;
  licenseNumber: string;
  title?: string | null;
  yearsOfExperience: number;
}

export const useRegisterDoctor = () =>
  useMutation({ mutationFn: (dto: RegisterDoctorDto) => api.post<DoctorProfile, DoctorProfile>('/doctors', dto) });

export const useSetAvailability = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (dto: { override: AvailabilityStatus | null; overrideExpiresAt?: string | null }) =>
      api.put<Availability, Availability>('/doctors/me/availability', dto),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEYS.me }),
  });
};

export const useSubmitVerification = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (input: { file: File; documentType: string; qrPayload?: string }) => {
      const form = new FormData();
      form.append('document', input.file);
      form.append('documentType', input.documentType);
      if (input.qrPayload) form.append('qrPayload', input.qrPayload);
      return api.post<Verification, Verification>('/doctors/me/verifications', form);
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: KEYS.me }),
  });
};

// --- Vaka üzerindeki doktor aksiyonları ---

export const useReviewAnalysis = (caseId: string) => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (input: { analysisId: string; decision: 'Approved' | 'Corrected'; correctionNotes?: string }) =>
      api.post(`/cases/${caseId}/analyses/${input.analysisId}/review`, {
        decision: input.decision,
        correctionNotes: input.correctionNotes,
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: caseKeys.analyses(caseId) }),
  });
};

export const useAddClinicalNote = (caseId: string, consultationId: string | undefined) =>
  useMutation({
    mutationFn: (content: string) =>
      api.post(`/cases/${caseId}/consultations/${consultationId}/clinical-notes`, { content }),
  });

export const useCreateTreatmentPlan = (caseId: string, consultationId: string | undefined) => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (dto: { description: string; followUpDate?: string | null }) =>
      api.post<Treatment, Treatment>(`/cases/${caseId}/consultations/${consultationId}/treatment-plan`, dto),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: caseKeys.detail(caseId) });
      qc.invalidateQueries({ queryKey: caseKeys.route(caseId) });
      qc.invalidateQueries({ queryKey: caseKeys.snapshots(caseId) });
    },
  });
};

export const useCompleteConsultation = (caseId: string, consultationId: string | undefined) => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: () => api.post<Consultation, Consultation>(`/cases/${caseId}/consultations/${consultationId}/complete`),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: caseKeys.consultations(caseId) });
      qc.invalidateQueries({ queryKey: KEYS.queue });
    },
  });
};

export const useRequestEscalation = (caseId: string) =>
  useMutation({ mutationFn: () => api.post(`/cases/${caseId}/escalation-request`) });

export const useCloseCase = (caseId: string) => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: () => api.post<Case, Case>(`/cases/${caseId}/close`),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: caseKeys.detail(caseId) });
      qc.invalidateQueries({ queryKey: KEYS.queue });
    },
  });
};

export const doctorKeys = KEYS;
