using System.Text;
using System.Threading.RateLimiting;
using EduVoice.Application.Interfaces;
using EduVoice.Application.Services;
using EduVoice.Application.Validators;
using EduVoice.Domain.Interfaces;
using EduVoice.Infrastructure.BackgroundServices;
using EduVoice.Infrastructure.Data;
using EduVoice.Infrastructure.Repositories;
using EduVoice.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

namespace EduVoice.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEduVoiceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext(configuration);
        services.AddRepositories();
        services.AddApplicationServices();
        services.AddInfrastructureServices();
        services.AddJwtAuthentication(configuration);
        services.AddSwaggerDocumentation();
        services.AddRateLimitingPolicies();
        services.AddCorsPolicy(configuration);
        services.AddValidatorsFromAssemblyContaining<LoginValidator>();
        services.AddHttpClients();
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("database");

        return services;
    }

    private static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["DATABASE_URL"]
            ?? throw new InvalidOperationException("DATABASE_URL is required");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                npgsql.CommandTimeout(30);
            });

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                options.EnableSensitiveDataLogging();
        });
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ICallService, CallService>();
        services.AddScoped<ICampaignService, CampaignService>();
        services.AddScoped<IComplaintService, ComplaintService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IInboundService, InboundService>();
        services.AddScoped<IWhatsAppBotService, WhatsAppBotService>();
        services.AddScoped<IBroadcastService, BroadcastService>();
        services.AddScoped<IFeeReceiptService, FeeReceiptService>();
        services.AddScoped<IExamScheduleService, ExamScheduleService>();
        services.AddScoped<IHomeworkService, HomeworkService>();
        services.AddScoped<ILowMarksAlertService, LowMarksAlertService>();
        services.AddScoped<IDropoutRiskService, DropoutRiskService>();
        services.AddScoped<IFeeCollectionService, FeeCollectionService>();
        services.AddScoped<IInstalmentService, InstalmentService>();
        services.AddScoped<IParentEngagementService, ParentEngagementService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IStaffOperationsService, StaffOperationsService>();
    }

    private static void AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IVapiService, VapiService>();
        services.AddScoped<ITwilioService, TwilioService>();
        services.AddScoped<IElevenLabsService, ElevenLabsService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<ClaudeAIService>();
        services.AddHostedService<RetryBackgroundService>();
        services.AddHostedService<CarrierHealthBackgroundService>();
        services.AddHostedService<MissedCallBackgroundService>();
        services.AddHostedService<ExamReminderBackgroundService>();
        services.AddHostedService<HomeworkAlertBackgroundService>();
        services.AddHostedService<DropoutRiskBackgroundService>();
        services.AddHostedService<DefaulterEscalationBackgroundService>();
        services.AddHostedService<InstalmentReminderBackgroundService>();
        services.AddHostedService<BirthdayWishBackgroundService>();
        services.AddHostedService<WeeklySummaryBackgroundService>();
        services.AddHostedService<PtmReminderBackgroundService>();
    }

    private static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSecret = configuration["JWT_SECRET"]
            ?? throw new InvalidOperationException("JWT_SECRET is required");
        var jwtIssuer = configuration["JWT_ISSUER"] ?? "EduVoice";
        var jwtAudience = configuration["JWT_AUDIENCE"] ?? "EduVoice";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        if (ctx.Exception is SecurityTokenExpiredException)
                            ctx.Response.Headers.Append("Token-Expired", "true");
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();
    }

    private static void AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "EduVoice API",
                Version = "v1",
                Description = "AI Voice Agent SaaS for Telugu Schools - Fee reminders, Progress updates, Complaint collection",
                Contact = new OpenApiContact
                {
                    Name = "EduVoice Support",
                    Email = "support@eduvoice.in"
                }
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header. Enter: Bearer {token}",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }

    private static void AddRateLimitingPolicies(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter("auth", limiterOptions =>
            {
                limiterOptions.PermitLimit = 5;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            options.AddFixedWindowLimiter("api", limiterOptions =>
            {
                limiterOptions.PermitLimit = 100;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 10;
            });
        });
    }

    private static void AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var frontendUrl = configuration["FRONTEND_URL"] ?? "http://localhost:4200";

        services.AddCors(options =>
        {
            options.AddPolicy("EduVoiceCors", policy =>
            {
                policy.WithOrigins(frontendUrl)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });

            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            });
        });
    }

    private static void AddHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient("vapi", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.BaseAddress = new Uri("https://api.vapi.ai/");
        });

        services.AddHttpClient("elevenlabs", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.BaseAddress = new Uri("https://api.elevenlabs.io/");
        });

        services.AddHttpClient("claude", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(60);
            client.BaseAddress = new Uri("https://api.anthropic.com/");
        });
    }
}
