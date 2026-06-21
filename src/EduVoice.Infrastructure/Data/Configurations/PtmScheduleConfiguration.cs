using EduVoice.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduVoice.Infrastructure.Data.Configurations;

public class PtmScheduleConfiguration : IEntityTypeConfiguration<PtmSchedule>
{
    public void Configure(EntityTypeBuilder<PtmSchedule> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Title).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Class).HasMaxLength(50);
        builder.Property(p => p.Section).HasMaxLength(10);
        builder.Property(p => p.Notes).HasMaxLength(1000);

        builder.HasOne(p => p.School)
            .WithMany()
            .HasForeignKey(p => p.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.SchoolId, p.PtmDate, p.IsActive });
    }
}
