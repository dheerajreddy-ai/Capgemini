using EduVoice.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduVoice.Infrastructure.Data.Configurations;

public class ComplaintConfiguration : IEntityTypeConfiguration<Complaint>
{
    public void Configure(EntityTypeBuilder<Complaint> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Summary)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.DetailedDescription)
            .HasMaxLength(5000);

        builder.Property(c => c.ParentName)
            .HasMaxLength(200);

        builder.Property(c => c.ParentPhone)
            .HasMaxLength(20);

        builder.Property(c => c.Resolution)
            .HasMaxLength(2000);

        builder.Property(c => c.Category)
            .HasConversion<string>();

        builder.Property(c => c.Priority)
            .HasConversion<string>();

        builder.Property(c => c.Status)
            .HasConversion<string>();

        builder.HasIndex(c => c.SchoolId);
        builder.HasIndex(c => new { c.SchoolId, c.Status });
        builder.HasIndex(c => new { c.SchoolId, c.Priority });

        builder.HasOne(c => c.Student)
            .WithMany(s => s.Complaints)
            .HasForeignKey(c => c.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("Complaints");
    }
}
