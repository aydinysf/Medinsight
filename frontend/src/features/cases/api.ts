import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '../../lib/api';
import type {
  AiAnalysis,
  Case,
  CaseDocument,
  Consultation,
  ConsultationMessage,
  DoctorMatch,
  HealthRoute,
  HizirChatMessage,
  HealthRouteSnapshot,
  ImageFinding,
  Patient,
  TimelineEntry,
} from '../../lib/types';

const KEYS = {
  me: ['patients', 'me'] as const,
  cases: (patientId: string) => ['patients', patientId, 'cases'] as const,
  detail: (id: string) => ['cases', id] as const,
  route: (id: string) => ['cases', id, 'health-route'] as const,
  snapshots: (id: string) => ['cases', id, 'snapshots'] as const,
  documents: (id: string) => ['cases', id, 'documents'] as const,
  analyses: (id: string) => ['cases', id, 'analyses'] as const,
  imageFindings: (id: string) => ['cases', id, 'image-findings'] as const,
  timeline: (id: string) => ['cases', id, 'timeline'] as const,
  matches: (id: string) => ['cases', id, 'doctor-matches'] as const,
  consultations: (id: string) => ['cases', id, 'consultations'] as const,
  hizirChat: (id: string) => ['cases', id, 'hizir-chat'] as const,
  messages: (id: string, consultationId: string) => ['cases', id, 'consultations', consultationId, 'messages'] as const,
};

export const useMe = () =>
  useQuery({ queryKey: KEYS.me, queryFn: () => api.get<Patient, Patient>('/patients/me') });

export const useCases = (patientId: string | undefined) =>
  useQuery({
    queryKey: KEYS.cases(patientId ?? ''),
    queryFn: () => api.get<Case[], Case[]>(`/patients/${patientId}/cases`),
    enabled: !!patientId,
  });

export const useCase = (id: string) =>
  useQuery({ queryKey: KEYS.detail(id), queryFn: () => api.get<Case, Case>(`/cases/${id}`), refetchInterval: 5000 });

export const useHealthRoute = (id: string) =>
  useQuery({
    queryKey: KEYS.route(id),
    queryFn: () => api.get<HealthRoute, HealthRoute>(`/cases/${id}/health-route`),
    refetchInterval: 5000,
  });

export const useSnapshots = (id: string) =>
  useQuery({
    queryKey: KEYS.snapshots(id),
    queryFn: () => api.get<HealthRouteSnapshot[], HealthRouteSnapshot[]>(`/cases/${id}/health-route/snapshots`),
  });

export const useDocuments = (id: string) =>
  useQuery({
    queryKey: KEYS.documents(id),
    queryFn: () => api.get<CaseDocument[], CaseDocument[]>(`/cases/${id}/documents`),
    refetchInterval: 5000,
  });

export const useAnalyses = (id: string) =>
  useQuery({
    queryKey: KEYS.analyses(id),
    queryFn: () => api.get<AiAnalysis[], AiAnalysis[]>(`/cases/${id}/analyses`),
    refetchInterval: 5000,
  });

export const useImageFindings = (id: string) =>
  useQuery({
    queryKey: KEYS.imageFindings(id),
    queryFn: () => api.get<ImageFinding[], ImageFinding[]>(`/cases/${id}/image-findings`),
    refetchInterval: 10000,
  });

export const useTimeline = (id: string) =>
  useQuery({
    queryKey: KEYS.timeline(id),
    queryFn: () => api.get<TimelineEntry[], TimelineEntry[]>(`/cases/${id}/timeline`),
    refetchInterval: 5000,
  });

export const useDoctorMatches = (id: string) =>
  useQuery({
    queryKey: KEYS.matches(id),
    queryFn: () => api.get<DoctorMatch[], DoctorMatch[]>(`/cases/${id}/doctor-matches`),
  });

export const useConsultations = (id: string) =>
  useQuery({
    queryKey: KEYS.consultations(id),
    queryFn: () => api.get<Consultation[], Consultation[]>(`/cases/${id}/consultations`),
  });

export const useMessages = (caseId: string, consultationId: string | undefined) =>
  useQuery({
    queryKey: KEYS.messages(caseId, consultationId ?? ''),
    queryFn: () =>
      api.get<ConsultationMessage[], ConsultationMessage[]>(`/cases/${caseId}/consultations/${consultationId}/messages`),
    enabled: !!consultationId,
  });

export const useHizirChat = (caseId: string) =>
  useQuery({
    queryKey: KEYS.hizirChat(caseId),
    queryFn: () => api.get<HizirChatMessage[], HizirChatMessage[]>(`/cases/${caseId}/hizir-chat`),
  });

export const useSendHizirMessage = (caseId: string) => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (message: string) =>
      api.post<HizirChatMessage, HizirChatMessage>(`/cases/${caseId}/hizir-chat`, { message }),
    // onSettled: hata durumunda da yenile — kullanıcı mesajı sunucuda kayıtlı olabilir.
    onSettled: () => qc.invalidateQueries({ queryKey: KEYS.hizirChat(caseId) }),
  });
};

export interface CreateCaseDto {
  patientId: string;
  title: string;
  description?: string;
  bodySystem?: string;
}

export const useCreateCase = (patientId: string | undefined) => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (dto: CreateCaseDto) => api.post<Case, Case>('/cases', dto),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEYS.cases(patientId ?? '') }),
  });
};

export const useUploadDocuments = (caseId: string) => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (files: File[]) => {
      const form = new FormData();
      files.forEach((f) => form.append('files', f));
      return api.post(`/cases/${caseId}/documents`, form, {
        headers: { 'Idempotency-Key': crypto.randomUUID() },
      });
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: KEYS.documents(caseId) });
      qc.invalidateQueries({ queryKey: KEYS.timeline(caseId) });
    },
  });
};

export const useStartConsultation = (caseId: string) => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (doctorId: string) =>
      api.post<Consultation, Consultation>(`/cases/${caseId}/consultations`, { doctorId }),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEYS.consultations(caseId) }),
  });
};

export const useSendMessage = (caseId: string, consultationId: string | undefined) => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (content: string) =>
      api.post<ConsultationMessage, ConsultationMessage>(
        `/cases/${caseId}/consultations/${consultationId}/messages`,
        { content },
      ),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEYS.messages(caseId, consultationId ?? '') }),
  });
};

export const caseKeys = KEYS;
