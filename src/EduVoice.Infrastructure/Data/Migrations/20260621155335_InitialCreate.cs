using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduVoice.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OldValues = table.Column<string>(type: "text", nullable: true),
                    NewValues = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Schools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SubDomain = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PrimaryColor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PlanType = table.Column<string>(type: "text", nullable: false),
                    StudentCount = table.Column<int>(type: "integer", nullable: false),
                    TwilioPhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    VapiAssistantId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ElevenLabsVoiceId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TeluguDialect = table.Column<string>(type: "text", nullable: false),
                    ElevenLabsVoiceIdAndhra = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UpiId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CarrierHealth = table.Column<string>(type: "text", nullable: false),
                    CarrierHealthCheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DndScrubEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    UrduVoiceId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DefaultCallLanguage = table.Column<string>(type: "text", nullable: false),
                    AttendanceAlertThreshold = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    LowMarksThreshold = table.Column<decimal>(type: "numeric", nullable: false),
                    AchievementThreshold = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TrialEndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PrincipalEmail = table.Column<string>(type: "text", nullable: true),
                    PrincipalWhatsApp = table.Column<string>(type: "text", nullable: true),
                    DailySummaryEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schools", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Broadcasts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    MediaUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MediaType = table.Column<string>(type: "text", nullable: false),
                    TargetClass = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TargetSection = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    TotalRecipients = table.Column<int>(type: "integer", nullable: false),
                    SentCount = table.Column<int>(type: "integer", nullable: false),
                    FailedCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Broadcasts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Broadcasts_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FilterClass = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FilterSection = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    FilterFeesStatus = table.Column<string>(type: "text", nullable: true),
                    TotalStudents = table.Column<int>(type: "integer", nullable: false),
                    CallsInitiated = table.Column<int>(type: "integer", nullable: false),
                    CallsCompleted = table.Column<int>(type: "integer", nullable: false),
                    CallsFailed = table.Column<int>(type: "integer", nullable: false),
                    CallsNoAnswer = table.Column<int>(type: "integer", nullable: false),
                    CustomMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WaveSize = table.Column<int>(type: "integer", nullable: false),
                    WaveGapMinutes = table.Column<int>(type: "integer", nullable: false),
                    CurrentWave = table.Column<int>(type: "integer", nullable: false),
                    CallsVoicemail = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Campaigns_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExamType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExamDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Class = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Section = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Reminder3DaySent = table.Column<bool>(type: "boolean", nullable: false),
                    Reminder1DaySent = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSchedules_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Homeworks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Class = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Section = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    AssignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlertSent = table.Column<bool>(type: "boolean", nullable: false),
                    AlertSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Homeworks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Homeworks_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PtmSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PtmDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Class = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Section = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Reminder3DaySent = table.Column<bool>(type: "boolean", nullable: false),
                    Reminder1DaySent = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PtmSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PtmSchedules_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffAbsences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TeacherPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SubstituteTeacherName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SubstituteTeacherPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AffectedClass = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AffectedSection = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    AbsenceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SubstituteAlertSent = table.Column<bool>(type: "boolean", nullable: false),
                    ParentNotificationSent = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffAbsences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffAbsences_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Class = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Section = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ParentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ParentPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ParentPhone2 = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ParentWhatsApp = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ParentEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MathMarks = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    ScienceMarks = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    EnglishMarks = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    TeluguMarks = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    SocialMarks = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    TotalMarks = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: true),
                    MaxMarks = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: true),
                    Percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    Grade = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AttendancePresentDays = table.Column<int>(type: "integer", nullable: true),
                    AttendanceTotalDays = table.Column<int>(type: "integer", nullable: true),
                    AttendancePercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    TotalFees = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidFees = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PendingFees = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FeesStatus = table.Column<string>(type: "text", nullable: false),
                    FeesDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastPaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DoNotCall = table.Column<bool>(type: "boolean", nullable: false),
                    DoNotCallSetAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HasFeeExtension = table.Column<bool>(type: "boolean", nullable: false),
                    FeeExtensionUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentLink = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PaymentLinkGeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HasFeeDispute = table.Column<bool>(type: "boolean", nullable: false),
                    FeeDisputeNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FeeDisputeRaisedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsScholarship = table.Column<bool>(type: "boolean", nullable: false),
                    ScholarshipNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ScholarshipPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    DefaulterEscalationLevel = table.Column<string>(type: "text", nullable: false),
                    DefaulterEscalatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NeedsPersonalFollowup = table.Column<bool>(type: "boolean", nullable: false),
                    LowMarksAlertSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WeeklySummarySentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AchievementAlertSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DropoutRiskScore = table.Column<int>(type: "integer", nullable: false),
                    DropoutRiskLevel = table.Column<string>(type: "text", nullable: false),
                    DropoutRiskCalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DropoutRiskReasons = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PortalOtpHash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PortalOtpExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastPortalAccessAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MissedCallCallbackPending = table.Column<bool>(type: "boolean", nullable: false),
                    MissedCallReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RefreshToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RefreshTokenExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PasswordResetExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Calls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: true),
                    VapiCallId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Direction = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ToPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FromPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    RecordingUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TranscriptJson = table.Column<string>(type: "text", nullable: true),
                    TranscriptText = table.Column<string>(type: "text", nullable: true),
                    Sentiment = table.Column<string>(type: "text", nullable: true),
                    FeesConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    HasComplaint = table.Column<bool>(type: "boolean", nullable: false),
                    ComplaintSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CallbackRequested = table.Column<bool>(type: "boolean", nullable: false),
                    AiSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    IsVoicemail = table.Column<bool>(type: "boolean", nullable: false),
                    RetryScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OriginalCallId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsPartialTranscript = table.Column<bool>(type: "boolean", nullable: false),
                    EscalationRequired = table.Column<bool>(type: "boolean", nullable: false),
                    EscalationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LowConfidenceTranscript = table.Column<bool>(type: "boolean", nullable: false),
                    DialectUsed = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Language = table.Column<string>(type: "text", nullable: false),
                    NetworkQuality = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Calls_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Calls_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Calls_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeeInstalments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstalmentNumber = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OverdueReminderSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeInstalments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeInstalments_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeeInstalments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attendances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    MarkedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsPresent = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attendances_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attendances_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attendances_Users_MarkedByUserId",
                        column: x => x.MarkedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintSlaConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    SlaHours = table.Column<int>(type: "integer", nullable: false),
                    EscalationContactUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EscalationContactId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintSlaConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintSlaConfigs_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComplaintSlaConfigs_Users_EscalationContactId",
                        column: x => x.EscalationContactId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TeacherMarksList",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamType = table.Column<int>(type: "integer", nullable: false),
                    ExamDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MathMarks = table.Column<decimal>(type: "numeric", nullable: true),
                    ScienceMarks = table.Column<decimal>(type: "numeric", nullable: true),
                    EnglishMarks = table.Column<decimal>(type: "numeric", nullable: true),
                    TeluguMarks = table.Column<decimal>(type: "numeric", nullable: true),
                    SocialMarks = table.Column<decimal>(type: "numeric", nullable: true),
                    MaxMarks = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherMarksList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherMarksList_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherMarksList_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherMarksList_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Complaints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CallId = table.Column<Guid>(type: "uuid", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DetailedDescription = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    ParentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ParentPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Resolution = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SlaDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EscalatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EscalationLevel = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Complaints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Complaints_Calls_CallId",
                        column: x => x.CallId,
                        principalTable: "Calls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Complaints_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Complaints_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_MarkedByUserId",
                table: "Attendances",
                column: "MarkedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_SchoolId",
                table: "Attendances",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_StudentId",
                table: "Attendances",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_SchoolId",
                table: "AuditLogs",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_SchoolId_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "SchoolId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Broadcasts_SchoolId",
                table: "Broadcasts",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Broadcasts_SchoolId_CreatedAt",
                table: "Broadcasts",
                columns: new[] { "SchoolId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Calls_CampaignId",
                table: "Calls",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Calls_SchoolId",
                table: "Calls",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Calls_SchoolId_CreatedAt",
                table: "Calls",
                columns: new[] { "SchoolId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Calls_SchoolId_Status",
                table: "Calls",
                columns: new[] { "SchoolId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Calls_StudentId",
                table: "Calls",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Calls_VapiCallId",
                table: "Calls",
                column: "VapiCallId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_SchoolId",
                table: "Campaigns",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_SchoolId_Status",
                table: "Campaigns",
                columns: new[] { "SchoolId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_CallId",
                table: "Complaints",
                column: "CallId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_SchoolId",
                table: "Complaints",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_SchoolId_Priority",
                table: "Complaints",
                columns: new[] { "SchoolId", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_SchoolId_Status",
                table: "Complaints",
                columns: new[] { "SchoolId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_StudentId",
                table: "Complaints",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintSlaConfigs_EscalationContactId",
                table: "ComplaintSlaConfigs",
                column: "EscalationContactId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintSlaConfigs_SchoolId",
                table: "ComplaintSlaConfigs",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSchedules_SchoolId_ExamDate",
                table: "ExamSchedules",
                columns: new[] { "SchoolId", "ExamDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FeeInstalments_SchoolId_DueDate_IsPaid",
                table: "FeeInstalments",
                columns: new[] { "SchoolId", "DueDate", "IsPaid" });

            migrationBuilder.CreateIndex(
                name: "IX_FeeInstalments_SchoolId_StudentId",
                table: "FeeInstalments",
                columns: new[] { "SchoolId", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_FeeInstalments_StudentId",
                table: "FeeInstalments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Homeworks_SchoolId_AssignedDate",
                table: "Homeworks",
                columns: new[] { "SchoolId", "AssignedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PtmSchedules_SchoolId_PtmDate_IsActive",
                table: "PtmSchedules",
                columns: new[] { "SchoolId", "PtmDate", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Schools_SubDomain",
                table: "Schools",
                column: "SubDomain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffAbsences_SchoolId_AbsenceDate",
                table: "StaffAbsences",
                columns: new[] { "SchoolId", "AbsenceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Students_SchoolId",
                table: "Students",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_SchoolId_Class_Section",
                table: "Students",
                columns: new[] { "SchoolId", "Class", "Section" });

            migrationBuilder.CreateIndex(
                name: "IX_Students_SchoolId_DropoutRiskLevel",
                table: "Students",
                columns: new[] { "SchoolId", "DropoutRiskLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_Students_SchoolId_FeesStatus",
                table: "Students",
                columns: new[] { "SchoolId", "FeesStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Students_SchoolId_NeedsPersonalFollowup",
                table: "Students",
                columns: new[] { "SchoolId", "NeedsPersonalFollowup" });

            migrationBuilder.CreateIndex(
                name: "IX_Students_SchoolId_StudentId",
                table: "Students",
                columns: new[] { "SchoolId", "StudentId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherMarksList_SchoolId",
                table: "TeacherMarksList",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherMarksList_StudentId",
                table: "TeacherMarksList",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherMarksList_UploadedByUserId",
                table: "TeacherMarksList",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_SchoolId",
                table: "Users",
                column: "SchoolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attendances");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Broadcasts");

            migrationBuilder.DropTable(
                name: "Complaints");

            migrationBuilder.DropTable(
                name: "ComplaintSlaConfigs");

            migrationBuilder.DropTable(
                name: "ExamSchedules");

            migrationBuilder.DropTable(
                name: "FeeInstalments");

            migrationBuilder.DropTable(
                name: "Homeworks");

            migrationBuilder.DropTable(
                name: "PtmSchedules");

            migrationBuilder.DropTable(
                name: "StaffAbsences");

            migrationBuilder.DropTable(
                name: "TeacherMarksList");

            migrationBuilder.DropTable(
                name: "Calls");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Schools");
        }
    }
}
