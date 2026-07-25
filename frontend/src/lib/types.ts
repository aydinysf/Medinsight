// Backend DTO karşılıkları — enum'lar JsonStringEnumConverter ile string gelir.

export type UserRole = 'Patient' | 'Caregiver' | 'Doctor' | 'Admin';

export interface LoginResult {
  accessToken: string;
  expiresAtUtc: string;
  userId: string;
  role: UserRole;
}

export interface Patient {
  id: string;
  userId: string;
  fullName: string;
  email: string;
  dateOfBirth: string | null;
  sex: string;
  createdAtUtc: string;
}

export type CaseStatus =
  | 'Draft'
  | 'CollectingData'
  | 'AIAnalysis'
  | 'DoctorReview'
  | 'Treatment'
  | 'FollowUp'
  | 'Closed';

export interface Case {
  id: string;
  patientId: string;
  title: string;
  description: string | null;
  bodySystem: string;
  status: CaseStatus;
  riskLevel: string;
  createdAtUtc: string;
}

export interface HealthRoute {
  caseId: string;
  currentVersionId: string;
  currentStatus: string;
  nextStep: string;
  riskLevel: string;
}

export interface HealthRouteSnapshot {
  id: string;
  previousVersionId: string | null;
  versionNumber: number;
  status: string;
  nextStep: string;
  riskLevel: string;
  triggeredBy: 'System' | 'AI' | 'Doctor' | 'Patient';
  reason: string;
  createdAtUtc: string;
}

export interface CaseDocument {
  id: string;
  title: string;
  type: string;
  status: string;
  originalFileName: string | null;
  sizeBytes: number;
  createdAtUtc: string;
}

export interface AiAnalysis {
  id: string;
  caseId: string;
  modelVersion: string;
  confidenceScore: number;
  summary: string;
  patientMessage: string;
  findings: { id: string; description: string; source: string }[];
  createdAtUtc: string;
}

export interface ImageFinding {
  id: string;
  modelName: string;
  outputType: string;
  description: string;
  disclaimer: string;
  createdAtUtc: string;
}

export interface TimelineEntry {
  id: string;
  caseId: string;
  eventType: string;
  occurredAt: string;
  summary: string;
}

export interface DoctorMatch {
  doctorId: string;
  fullName: string;
  title: string | null;
  specialty: string;
  score: number;
  scoreBreakdown: Record<string, number>;
  availabilityTag: 'Available' | 'Busy';
}

export interface Consultation {
  id: string;
  caseId: string;
  doctorId: string;
  status: 'Pending' | 'Active' | 'Completed';
  startedAtUtc: string;
  completedAtUtc: string | null;
}

export interface ConsultationMessage {
  id: string;
  consultationId: string;
  senderUserId: string;
  content: string;
  sentAtUtc: string;
}
