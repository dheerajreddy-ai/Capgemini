using EduVoice.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduVoice.Infrastructure.Data.Configurations;

public class FeeInstalmentConfiguration : IEntityTypeConfiguration<FeeInstalment>
{
    public void Configure(EntityTypeBuilder<FeeInstalment> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Amount).HasPrecision(18, 2).IsRequired();

        builder.HasOne(f => f.School)
            .WithMany()
            .HasForeignKey(f => f.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Student)
            .WithMany()
            .HasForeignKey(f => f.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => new { f.SchoolId, f.StudentId });
        builder.HasIndex(f => new { f.SchoolId, f.DueDate, f.IsPaid });
    }
}
