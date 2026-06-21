using EduVoice.Domain.Entities;
using EduVoice.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduVoice.Infrastructure.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context, ILogger logger)
    {
        try
        {
            var superAdminExists = await context.Users.AnyAsync(u => u.Email == "admin@eduvoice.in");
            if (!superAdminExists)
            {
                var superAdmin = new User
                {
                    Id = Guid.NewGuid(),
                    SchoolId = null,
                    Email = "admin@eduvoice.in",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("EduVoice@2024!", 12),
                    FirstName = "Super",
                    LastName = "Admin",
                    Role = UserRole.SuperAdmin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await context.Users.AddAsync(superAdmin);
                logger.LogInformation("Created SuperAdmin user: admin@eduvoice.in");
            }

            var demoSchoolExists = await context.Schools.AnyAsync(s => s.SubDomain == "demo");
            if (!demoSchoolExists)
            {
                var demoSchool = new School
                {
                    Id = Guid.NewGuid(),
                    Name = "Demo School",
                    SubDomain = "demo",
                    ContactEmail = "demo@demoschool.in",
                    ContactPhone = "+91-9876543210",
                    City = "Hyderabad",
                    State = "Telangana",
                    PlanType = PlanType.Growth,
                    IsActive = true,
                    TrialEndsAt = DateTime.UtcNow.AddDays(30),
                    StudentCount = 5,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await context.Schools.AddAsync(demoSchool);

                var demoAdmin = new User
                {
                    Id = Guid.NewGuid(),
                    SchoolId = demoSchool.Id,
                    Email = "admin@demo.in",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo@123456", 12),
                    FirstName = "Demo",
                    LastName = "Admin",
                    Role = UserRole.SchoolAdmin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await context.Users.AddAsync(demoAdmin);

                var demoStudents = new[]
                {
                    new Student
                    {
                        Id = Guid.NewGuid(),
                        SchoolId = demoSchool.Id,
                        StudentId = "STU001",
                        FirstName = "Ravi",
                        LastName = "Kumar",
                        Class = "10",
                        Section = "A",
                        ParentName = "Suresh Kumar",
                        ParentPhone = "+91-9000000001",
                        ParentEmail = "suresh@example.com",
                        MathMarks = 85,
                        ScienceMarks = 78,
                        EnglishMarks = 72,
                        TeluguMarks = 90,
                        SocialMarks = 80,
                        TotalMarks = 405,
                        MaxMarks = 500,
                        Percentage = 81,
                        Grade = "A",
                        AttendancePresentDays = 180,
                        AttendanceTotalDays = 200,
                        AttendancePercentage = 90,
                        TotalFees = 25000,
                        PaidFees = 25000,
                        PendingFees = 0,
                        FeesStatus = FeesStatus.Paid,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Student
                    {
                        Id = Guid.NewGuid(),
                        SchoolId = demoSchool.Id,
                        StudentId = "STU002",
                        FirstName = "Priya",
                        LastName = "Reddy",
                        Class = "10",
                        Section = "A",
                        ParentName = "Venkat Reddy",
                        ParentPhone = "+91-9000000002",
                        MathMarks = 92,
                        ScienceMarks = 88,
                        EnglishMarks = 85,
                        TeluguMarks = 95,
                        SocialMarks = 89,
                        TotalMarks = 449,
                        MaxMarks = 500,
                        Percentage = 89.8m,
                        Grade = "A",
                        AttendancePresentDays = 195,
                        AttendanceTotalDays = 200,
                        AttendancePercentage = 97.5m,
                        TotalFees = 25000,
                        PaidFees = 15000,
                        PendingFees = 10000,
                        FeesStatus = FeesStatus.Partial,
                        FeesDueDate = DateTime.UtcNow.AddDays(15),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Student
                    {
                        Id = Guid.NewGuid(),
                        SchoolId = demoSchool.Id,
                        StudentId = "STU003",
                        FirstName = "Arjun",
                        LastName = "Sharma",
                        Class = "9",
                        Section = "B",
                        ParentName = "Ramesh Sharma",
                        ParentPhone = "+91-9000000003",
                        MathMarks = 65,
                        ScienceMarks = 70,
                        EnglishMarks = 60,
                        TeluguMarks = 75,
                        SocialMarks = 68,
                        TotalMarks = 338,
                        MaxMarks = 500,
                        Percentage = 67.6m,
                        Grade = "B",
                        AttendancePresentDays = 160,
                        AttendanceTotalDays = 200,
                        AttendancePercentage = 80,
                        TotalFees = 22000,
                        PaidFees = 0,
                        PendingFees = 22000,
                        FeesStatus = FeesStatus.Overdue,
                        FeesDueDate = DateTime.UtcNow.AddDays(-10),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Student
                    {
                        Id = Guid.NewGuid(),
                        SchoolId = demoSchool.Id,
                        StudentId = "STU004",
                        FirstName = "Lakshmi",
                        LastName = "Devi",
                        Class = "8",
                        Section = "A",
                        ParentName = "Krishna Devi",
                        ParentPhone = "+91-9000000004",
                        MathMarks = 78,
                        ScienceMarks = 82,
                        EnglishMarks = 75,
                        TeluguMarks = 88,
                        SocialMarks = 76,
                        TotalMarks = 399,
                        MaxMarks = 500,
                        Percentage = 79.8m,
                        Grade = "B+",
                        AttendancePresentDays = 185,
                        AttendanceTotalDays = 200,
                        AttendancePercentage = 92.5m,
                        TotalFees = 20000,
                        PaidFees = 10000,
                        PendingFees = 10000,
                        FeesStatus = FeesStatus.Unpaid,
                        FeesDueDate = DateTime.UtcNow.AddDays(5),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Student
                    {
                        Id = Guid.NewGuid(),
                        SchoolId = demoSchool.Id,
                        StudentId = "STU005",
                        FirstName = "Sai",
                        LastName = "Chandra",
                        Class = "9",
                        Section = "A",
                        ParentName = "Narayana Chandra",
                        ParentPhone = "+91-9000000005",
                        MathMarks = 95,
                        ScienceMarks = 92,
                        EnglishMarks = 89,
                        TeluguMarks = 97,
                        SocialMarks = 93,
                        TotalMarks = 466,
                        MaxMarks = 500,
                        Percentage = 93.2m,
                        Grade = "A+",
                        AttendancePresentDays = 200,
                        AttendanceTotalDays = 200,
                        AttendancePercentage = 100,
                        TotalFees = 22000,
                        PaidFees = 22000,
                        PendingFees = 0,
                        FeesStatus = FeesStatus.Paid,
                        LastPaymentDate = DateTime.UtcNow.AddDays(-5),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                await context.Students.AddRangeAsync(demoStudents);
                logger.LogInformation("Created demo school with {Count} students", demoStudents.Length);
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Database seeding completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding database");
            throw;
        }
    }
}
