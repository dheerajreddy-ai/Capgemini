using EduVoice.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduVoice.Infrastructure.Data.Configurations;

public class ExamScheduleConfiguration : IEntityTypeConfiguration<ExamSchedule>
{
    public void Configure(EntityTypeBuilder<ExamSchedule> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.SubjectName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.ExamType).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Class).HasMaxLength(20);
        builder.Property(e => e.Section).HasMaxLength(10);
        builder.Property(e => e.Notes).HasMaxLength(500);

        builder.HasOne(e => e.School)
            .WithMany()
            .HasForeignKey(e => e.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.SchoolId, e.ExamDate });
    }
}
