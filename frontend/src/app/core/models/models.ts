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
export type CampaignType = 'FeeReminder' | 'ProgressUpdate' | 'Custom';
export type CampaignStatus = 'Draft' | 'Running' | 'Paused' | 'Completed' | 'Failed';
export type CallType = 'FeeReminder' | 'ProgressUpdate' | 'Complaint' | 'Inbound';
export type CallStatus = 'Initiated' | 'Ringing' | 'InProgress' | 'Completed' | 'Failed' | 'NoAnswer' | 'Busy';
export type Sentiment = 'Positive' | 'Neutral' | 'Negative' | 'Angry';
export type ComplaintCategory = 'Teacher' | 'Fees' | 'Facility' | 'Academic' | 'Behaviour' | 'Other';
export type ComplaintPriority = 'Low' | 'Medium' | 'High' | 'Urgent';
export type ComplaintStatus = 'New' | 'Read' | 'InProgress' | 'Resolved' | 'Closed';

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
}

export interface Campaign {
  id: string;
  campaignName: string;
  campaignType: CampaignType;
  status: CampaignStatus;
  totalStudents: number;
  callsInitiated: number;
  callsCompleted: number;
  callsFailed: number;
  callsNoAnswer: number;
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
  studentClass: string;
  parentName: string;
  parentPhone: string;
  campaignId?: string;
  campaignName?: string;
  callType: CallType;
  status: CallStatus;
  durationSeconds: number;
  transcript?: string;
  transcriptJson?: TranscriptMessage[];
  recordingUrl?: string;
  summary?: string;
  sentiment: Sentiment;
  sentimentScore?: number;
  hasComplaint: boolean;
  feesConfirmed: boolean;
  callbackRequested: boolean;
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
