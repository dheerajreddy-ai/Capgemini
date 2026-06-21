using EduVoice.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduVoice.Infrastructure.Data.Configurations;

public class StaffAbsenceConfiguration : IEntityTypeConfiguration<StaffAbsence>
{
    public void Configure(EntityTypeBuilder<StaffAbsence> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.TeacherName).HasMaxLength(200).IsRequired();
        builder.Property(s => s.TeacherPhone).HasMaxLength(20);
        builder.Property(s => s.SubstituteTeacherName).HasMaxLength(200);
        builder.Property(s => s.SubstituteTeacherPhone).HasMaxLength(20);
        builder.Property(s => s.AffectedClass).HasMaxLength(50);
        builder.Property(s => s.AffectedSection).HasMaxLength(10);
        builder.Property(s => s.Notes).HasMaxLength(1000);

        builder.HasOne(s => s.School)
            .WithMany()
            .HasForeignKey(s => s.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.SchoolId, s.AbsenceDate });
    }
}
