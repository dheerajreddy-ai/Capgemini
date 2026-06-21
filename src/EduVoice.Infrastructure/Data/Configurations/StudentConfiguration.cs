using EduVoice.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduVoice.Infrastructure.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.StudentId)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(s => new { s.SchoolId, s.StudentId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.Property(s => s.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.LastName)
            .HasMaxLength(100);

        builder.Property(s => s.Class)
            .HasMaxLength(50);

        builder.Property(s => s.Section)
            .HasMaxLength(10);

        builder.Property(s => s.ParentName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.ParentPhone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.ParentPhone2)
            .HasMaxLength(20);

        builder.Property(s => s.ParentWhatsApp)
            .HasMaxLength(20);

        builder.Property(s => s.ParentEmail)
            .HasMaxLength(200);

        builder.Property(s => s.Address)
            .HasMaxLength(500);

        builder.Property(s => s.Grade)
            .HasMaxLength(5);

        builder.Property(s => s.Remarks)
            .HasMaxLength(1000);

        builder.Property(s => s.TotalFees)
            .HasPrecision(18, 2);

        builder.Property(s => s.PaidFees)
            .HasPrecision(18, 2);

        builder.Property(s => s.PendingFees)
            .HasPrecision(18, 2);

        builder.Property(s => s.MathMarks).HasPrecision(5, 2);
        builder.Property(s => s.ScienceMarks).HasPrecision(5, 2);
        builder.Property(s => s.EnglishMarks).HasPrecision(5, 2);
        builder.Property(s => s.TeluguMarks).HasPrecision(5, 2);
        builder.Property(s => s.SocialMarks).HasPrecision(5, 2);
        builder.Property(s => s.TotalMarks).HasPrecision(7, 2);
        builder.Property(s => s.MaxMarks).HasPrecision(7, 2);
        builder.Property(s => s.Percentage).HasPrecision(5, 2);
        builder.Property(s => s.AttendancePercentage).HasPrecision(5, 2);

        builder.Property(s => s.FeesStatus)
            .HasConversion<string>();

        builder.Property(s => s.PaymentLink)
            .HasMaxLength(500);

        builder.Property(s => s.FeeDisputeNote)
            .HasMaxLength(1000);

        builder.Property(s => s.PortalOtpHash)
            .HasMaxLength(200);

        builder.Property(s => s.ScholarshipNote).HasMaxLength(500);
        builder.Property(s => s.ScholarshipPercent).HasPrecision(5, 2);

        builder.Property(s => s.DefaulterEscalationLevel).HasConversion<string>();
        builder.HasIndex(s => new { s.SchoolId, s.NeedsPersonalFollowup });

        builder.Property(s => s.DropoutRiskLevel).HasConversion<string>();

        builder.Property(s => s.DropoutRiskReasons)
            .HasMaxLength(500);

        builder.HasIndex(s => new { s.SchoolId, s.DropoutRiskLevel });
        builder.HasIndex(s => s.SchoolId);
        builder.HasIndex(s => new { s.SchoolId, s.Class, s.Section });
        builder.HasIndex(s => new { s.SchoolId, s.FeesStatus });

        builder.HasQueryFilter(s => !s.IsDeleted);

        builder.ToTable("Students");
    }
}
