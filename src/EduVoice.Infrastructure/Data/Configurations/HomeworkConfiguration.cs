using EduVoice.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduVoice.Infrastructure.Data.Configurations;

public class HomeworkConfiguration : IEntityTypeConfiguration<Homework>
{
    public void Configure(EntityTypeBuilder<Homework> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Subject).HasMaxLength(100).IsRequired();
        builder.Property(h => h.Description).HasMaxLength(2000).IsRequired();
        builder.Property(h => h.Class).HasMaxLength(20);
        builder.Property(h => h.Section).HasMaxLength(10);

        builder.HasOne(h => h.School)
            .WithMany()
            .HasForeignKey(h => h.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(h => new { h.SchoolId, h.AssignedDate });
    }
}
