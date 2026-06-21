/* ============================================================
   EduVoice — Shared domain models (mirror backend DTOs)
   ============================================================ */

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  code?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNext: boolean;
  hasPrev: boolean;
}

export type PlanType = 'Starter' | 'Growth' | 'Pro';
export type UserRole = 'SuperAdmin' | 'SchoolAdmin' | 'Teacher' | 'Viewer';
export type FeesStatus = 'Paid' | 'Partial' | 'Unpaid' | 'Overdue';
export type CampaignType = 'FeeReminder' | 'ProgressUpdate' | 'Custom' | 'AttendanceAlert';
export type CampaignStatus = 'Draft' | 'Scheduled' | 'Running' | 'Paused' | 'Completed' | 'Failed';
export type CallType = 'FeeReminder' | 'ProgressUpdate' | 'Complaint' | 'Inbound' | 'AttendanceAlert';
export type CallStatus = 'Initiated' | 'Ringing' | 'InProgress' | 'Completed' | 'Failed' | 'NoAnswer' | 'Busy';
export type Sentiment = 'Positive' | 'Neutral' | 'Negative' | 'Angry';
export type ComplaintCategory = 'Teacher' | 'Fees' | 'Facility' | 'Academic' | 'Behaviour' | 'Other';
export type ComplaintPriority = 'Low' | 'Medium' | 'High' | 'Urgent';
export type ComplaintStatus = 'New' | 'Read' | 'InProgress' | 'Resolved' | 'Closed';
export type TeluguDialect = 'Telangana' | 'Andhra';
export type CallLanguage = 'Telugu' | 'Urdu' | 'English';
export type CarrierHealth = 'Healthy' | 'Degraded' | 'Flagged';
export type BroadcastStatus = 'Draft' | 'Sending' | 'Sent' | 'Failed';
export type BroadcastMediaType = 'None' | 'Image' | 'Document' | 'Video';
export type ExamType = 'UnitTest' | 'Midterm' | 'Final' | 'Quarterly' | 'HalfYearly' | 'Annual';
export type DropoutRiskLevel = 'Low' | 'Medium' | 'High' | 'Critical';
export type DefaulterEscalationLevel = 'None' | 'Day30' | 'Day60' | 'Day90';

export interface FeeInstalment {
  id: string;
  instalmentNumber: number;
  amount: number;
  dueDate: string;
  isPaid: boolean;
  paidAt?: string;
  isOverdue: boolean;
  daysOverdue: number;
}

export interface InstalmentPlan {
  studentId: string;
  studentName: string;
  totalPending: number;
  totalInstalments: number;
  paidInstalments: number;
  overdueInstalments: number;
  instalments: FeeInstalment[];
}

export interface Broadcast {
  id: string;
  title: string;
  message: string;
  mediaUrl?: string;
  mediaType: BroadcastMediaType;
  targetClass?: string;
  targetSection?: string;
  status: BroadcastStatus;
  totalRecipients: number;
  sentCount: number;
  failedCount: number;
  sentAt?: string;
  createdAt: string;
}

export interface FeeCollectionDashboard {
  totalFeesExpected: number;
  totalFeesCollected: number;
  totalFeesPending: number;
  collectionRatePercent: number;
  collectedThisMonth: number;
  collectedLastMonth: number;
  monthOnMonthChange: number;
  totalStudents: number;
  paidCount: number;
  partialCount: number;
  unpaidCount: number;
  overdueCount: number;
  byClass: ClassCollectionStat[];
  topDefaulters: DefaulterStudent[];
  dailyRevenue: DailyRevenue[];
}

export interface ClassCollectionStat {
  class: string;
  studentCount: number;
  totalExpected: number;
  totalCollected: number;
  collectionRatePercent: number;
}

export interface DefaulterStudent {
  studentId: string;
  studentName: string;
  class?: string;
  section?: string;
  parentPhone: string;
  pendingFees: number;
  feesStatus: FeesStatus;
  feesDueDate?: string;
  daysOverdue: number;
  defaulterEscalationLevel: DefaulterEscalationLevel;
  needsPersonalFollowup: boolean;
}

export interface DailyRevenue {
  date: string;
  amount: number;
  label: string;
}

export interface DropoutRiskStudent {
  studentId: string;
  studentName: string;
  class?: string;
  section?: string;
  parentPhone: string;
  riskScore: number;
  riskLevel: DropoutRiskLevel;
  riskReasons: string[];
  attendancePercentage?: number;
  academicPercentage?: number;
  feesStatus: FeesStatus;
  pendingFees: number;
  noAnswerCallsLast30Days: number;
  calculatedAt?: string;
}

export interface DropoutRiskSummary {
  totalStudents: number;
  criticalCount: number;
  highCount: number;
  mediumCount: number;
  lowCount: number;
  atRiskStudents: DropoutRiskStudent[];
}

export interface ExamSchedule {
  id: string;
  subjectName: string;
  examType: ExamType;
  examDate: string;
  class?: string;
  section?: string;
  notes?: string;
  reminder3DaySent: boolean;
  reminder1DaySent: boolean;
  isActive: boolean;
  createdAt: string;
}

export interface Homework {
  id: string;
  subject: string;
  description: string;
  class?: string;
  section?: string;
  assignedDate: string;
  dueDate?: string;
  alertSent: boolean;
  alertSentAt?: string;
  createdAt: string;
}

export interface School {
  id: string;
  name: string;
  logoUrl?: string;
  subDomain: string;
  primaryColor?: string;
  contactEmail: string;
  contactPhone: string;
  address?: string;
  city?: string;
  state?: string;
  planType: PlanType;
  studentCount: number;
  twilioPhoneNumber?: string;
  vapiAssistantId?: string;
  elevenLabsVoiceId?: string;
  teluguDialect?: TeluguDialect;
  elevenLabsVoiceIdAndhra?: string;
  upiId?: string;
  urduVoiceId?: string;
  defaultCallLanguage?: CallLanguage;
  attendanceAlertThreshold?: number;
  lowMarksThreshold?: number;
  dndScrubEnabled?: boolean;
  carrierHealth?: CarrierHealth;
  isActive: boolean;
  trialEndsAt?: string;
}

export interface User {
  id: string;
  schoolId?: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  isActive: boolean;
  lastLoginAt?: string;
}

export interface AuthResult {
  accessToken: string;
  refreshToken: string;
  user: User;
  school?: School;
  expiresIn: number;
}

export interface Student {
  id: string;
  studentCode: string;
  fullName: string;
  class: string;
  section: string;
  parentName: string;
  parentPhone: string;
  parentPhone2?: string;
  parentWhatsApp?: string;
  feesDue: number;
  feesDueDate?: string;
  feesStatus: FeesStatus;
  mathsMarks?: number;
  scienceMarks?: number;
  englishMarks?: number;
  teluguMarks?: number;
  hindiMarks?: number;
  socialMarks?: number;
  attendance?: number;
  lastExamDate?: string;
  lastCalledAt?: string;
  notes?: string;
  isActive: boolean;
  // Phase 1
  doNotCall?: boolean;
  doNotCallSetAt?: string;
  hasFeeExtension?: boolean;
  feeExtensionUntil?: string;
  // Phase 4
  paymentLink?: string;
  paymentLinkGeneratedAt?: string;
  hasFeeDispute?: boolean;
  feeDisputeNote?: string;
  feeDisputeRaisedAt?: string;
  // Phase 14
  isScholarship?: boolean;
  scholarshipNote?: string;
  scholarshipPercent?: number;
  defaulterEscalationLevel?: DefaulterEscalationLevel;
  needsPersonalFollowup?: boolean;
}

export interface CallingWindowStatus {
  isOpen: boolean;
  window: string;
  nextOpenUtc?: string;
}

export interface Campaign {
  id: string;
  name: string;
  type: CampaignType;
  status: CampaignStatus;
  description?: string;
  totalStudents: number;
  callsInitiated: number;
  callsCompleted: number;
  callsFailed: number;
  callsNoAnswer: number;
  progressPercent: number;
  scheduledAt?: string;
  startedAt?: string;
  completedAt?: string;
  createdAt: string;
}

export interface TranscriptMessage {
  speaker: 'assistant' | 'user';
  text: string;
  timestamp: number;
}

export interface Call {
  id: string;
  studentId: string;
  studentName: string;
  studentClass?: string;
  section?: string;
  parentName?: string;
  parentPhone?: string;
  campaignId?: string;
  campaignName?: string;
  type: CallType;
  direction: 'Outbound' | 'Inbound';
  status: CallStatus;
  toPhone: string;
  durationSeconds?: number;
  transcript?: string;
  transcriptJson?: TranscriptMessage[];
  recordingUrl?: string;
  aiSummary?: string;
  sentiment?: Sentiment;
  hasComplaint: boolean;
  feesConfirmed: boolean;
  callbackRequested: boolean;
  retryCount: number;
  isVoicemail?: boolean;
  isPartialTranscript?: boolean;
  retryScheduledAt?: string;
  escalationRequired?: boolean;
  escalationReason?: string;
  lowConfidenceTranscript?: boolean;
  dialectUsed?: string;
  networkQuality?: string;
  startedAt?: string;
  endedAt?: string;
  createdAt: string;
}

export interface Complaint {
  id: string;
  studentId: string;
  studentName: string;
  studentClass: string;
  callId: string;
  parentName: string;
  parentPhone: string;
  complaintText: string;
  complaintSummary: string;
  category: ComplaintCategory;
  priority: ComplaintPriority;
  status: ComplaintStatus;
  assignedToUserId?: string;
  assignedToName?: string;
  teacherNotes?: string;
  recordingUrl?: string;
  resolvedAt?: string;
  createdAt: string;
}

export interface DashboardStats {
  todaysCalls: number;
  todaysCallsTrend: number;
  weeksCalls: number;
  feesConfirmedToday: number;
  feesConfirmedAmount: number;
  complaintsNew: number;
  pendingFollowups: number;
  callsThisWeek: { date: string; completed: number; noAnswer: number }[];
  sentimentBreakdown: { positive: number; neutral: number; negative: number };
  outcomeBreakdown: { feesConfirmed: number; complaintFiled: number; noAnswer: number; callbackRequested: number };
  campaignsSummary: Campaign[];
  recentCalls: Call[];
}
