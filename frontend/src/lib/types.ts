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
  differentialDiagnoses: { id: string; name: string; confidenceScore: number; riskLevel: string }[];
  createdAtUtc: string;
  reviewDecision: 'Approved' | 'Corrected' | null;
  reviewedByDoctorId: string | null;
  reviewedAtUtc: string | null;
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

export type AvailabilityStatus = 'Available' | 'Busy' | 'Away';

export type VerificationStatus = 'Pending' | 'Verified' | 'Rejected';

export interface DoctorProfile {
  id: string;
  userId: string;
  fullName: string;
  title: string | null;
  specialty: string;
  licenseNumber: string;
  yearsOfExperience: number;
  verificationStatus: VerificationStatus;
  effectiveStatus: AvailabilityStatus;
  activeCaseCount: number;
  capacityThreshold: number;
}

export interface Availability {
  effectiveStatus: AvailabilityStatus;
  computedStatus: AvailabilityStatus;
  manualOverride: AvailabilityStatus | null;
  overrideExpiresAt: string | null;
  activeCaseCount: number;
  capacityThreshold: number;
}

export interface Verification {
  id: string;
  doctorId: string;
  documentType: string;
  method: string;
  status: VerificationStatus;
  rejectionReason: string | null;
  createdAtUtc: string;
}

export interface DoctorMe {
  profile: DoctorProfile;
  availability: Availability;
  verifications: Verification[];
}

export interface PendingVerification {
  verificationId: string;
  doctorId: string;
  doctorFullName: string;
  specialty: string;
  licenseNumber: string;
  documentType: string;
  documentUrl: string;
  qrParsedData: string | null;
  submittedAtUtc: string;
}

export interface AuditLog {
  id: string;
  actorId: string | null;
  action: string;
  entityType: string | null;
  entityId: string | null;
  occurredAtUtc: string;
  ipAddress: string | null;
  metadataJson: string;
  correlationId: string;
}

export type ReviewPriority = 'Normal' | 'High';

export interface DoctorQueueItem {
  case: Case;
  reviewPriority: ReviewPriority;
  consultationId: string;
  consultationStatus: 'Pending' | 'Active' | 'Completed';
  consultationStartedAtUtc: string;
}

export interface Treatment {
  id: string;
  caseId: string;
  consultationId: string;
  createdByDoctorId: string;
  description: string;
  followUpDate: string | null;
  createdAtUtc: string;
}

export interface ConsultationMessage {
  id: string;
  consultationId: string;
  senderUserId: string;
  content: string;
  sentAtUtc: string;
}
