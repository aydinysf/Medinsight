import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '../../lib/api';
import type { AuditLog, PendingVerification, Verification } from '../../lib/types';

const KEYS = {
  pending: ['admin', 'doctor-verifications'] as const,
  audit: (entityId: string) => ['admin', 'audit-logs', entityId] as const,
};

export const usePendingVerifications = () =>
  useQuery({
    queryKey: KEYS.pending,
    queryFn: () => api.get<PendingVerification[], PendingVerification[]>('/admin/doctor-verifications'),
    refetchInterval: 10000,
  });

export const useApproveVerification = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (verificationId: string) =>
      api.post<Verification, Verification>(`/admin/doctor-verifications/${verificationId}/approve`, undefined, {
        headers: { 'Idempotency-Key': crypto.randomUUID() },
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEYS.pending }),
  });
};

export const useRejectVerification = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (input: { verificationId: string; reason: string }) =>
      api.post<Verification, Verification>(`/admin/doctor-verifications/${input.verificationId}/reject`, {
        reason: input.reason,
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEYS.pending }),
  });
};

/** Belgeyi API'den blob olarak indirir ve yeni sekmede açar (JWT korumalı). */
export const openVerificationDocument = async (verificationId: string) => {
  const blob = await api.get<Blob, Blob>(`/admin/doctor-verifications/${verificationId}/document`, {
    responseType: 'blob',
  });
  const url = URL.createObjectURL(blob);
  window.open(url, '_blank', 'noopener');
  setTimeout(() => URL.revokeObjectURL(url), 60_000);
};

export const useAuditLogs = (entityId: string, take = 50) =>
  useQuery({
    queryKey: KEYS.audit(entityId || 'all'),
    queryFn: () =>
      api.get<AuditLog[], AuditLog[]>('/admin/audit-logs', {
        params: { entityId: entityId || undefined, take },
      }),
    refetchInterval: 15000,
  });
