import type { CaseStatus } from '../../lib/types';

export const statusLabels: Record<CaseStatus, string> = {
  Draft: 'Taslak — belge bekleniyor',
  CollectingData: 'Veri toplanıyor',
  AIAnalysis: 'AI ön analizi yapılıyor',
  DoctorReview: 'Doktor incelemesinde',
  Treatment: 'Tedavi sürecinde',
  FollowUp: 'Takipte',
  Closed: 'Kapalı',
};

export const triggerLabels: Record<string, string> = {
  System: 'Sistem',
  AI: 'Hızır (AI)',
  Doctor: 'Doktor',
  Patient: 'Hasta',
};

export const bodySystems = [
  { value: 'Unknown', label: 'Bilmiyorum' },
  { value: 'Neuro', label: 'Sinir sistemi / Beyin' },
  { value: 'Cardio', label: 'Kalp ve damar' },
  { value: 'Oncology', label: 'Onkoloji' },
  { value: 'Endocrine', label: 'Hormonal / Endokrin' },
  { value: 'Orthopedic', label: 'Kemik ve eklem' },
  { value: 'Other', label: 'Diğer' },
];
