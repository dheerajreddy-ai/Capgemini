namespace EduVoice.Domain.Enums;

public enum PlanType { Starter, Growth, Pro }
public enum UserRole { SuperAdmin, SchoolAdmin, Teacher, Viewer }
public enum FeesStatus { Paid, Partial, Unpaid, Overdue }
public enum CampaignType { FeeReminder, ProgressUpdate, Custom, AttendanceAlert }
public enum CampaignStatus { Draft, Scheduled, Running, Paused, Completed, Failed }
public enum CallType { FeeReminder, ProgressUpdate, Complaint, Inbound, AttendanceAlert }
public enum CallStatus { Initiated, Ringing, InProgress, Completed, Failed, NoAnswer, Busy }
public enum CallDirection { Outbound, Inbound }
public enum SentimentType { Positive, Neutral, Negative, Angry }
public enum ComplaintCategory { Teacher, Fees, Facility, Academic, Behaviour, Other }
public enum ComplaintPriority { Low, Medium, High, Urgent }
public enum ComplaintStatus { New, Read, InProgress, Resolved, Closed }
public enum TeluguDialect { Telangana, Andhra }
public enum CallLanguage { Telugu, Urdu, English }
public enum CarrierHealth { Healthy, Degraded, Flagged }
public enum BroadcastStatus { Draft, Sending, Sent, Failed }
public enum BroadcastMediaType { None, Image, Document, Video }
