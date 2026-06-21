using EduVoice.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduVoice.Infrastructure.Data.Configurations;

public class BroadcastConfiguration : IEntityTypeConfiguration<Broadcast>
{
    public void Configure(EntityTypeBuilder<Broadcast> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Message)
            .IsRequired()
            .HasMaxLength(4096);

        builder.Property(b => b.MediaUrl)
            .HasMaxLength(500);

        builder.Property(b => b.TargetClass)
            .HasMaxLength(50);

        builder.Property(b => b.TargetSection)
            .HasMaxLength(10);

        builder.Property(b => b.Status)
            .HasConversion<string>();

        builder.Property(b => b.MediaType)
            .HasConversion<string>();

        builder.HasIndex(b => b.SchoolId);
        builder.HasIndex(b => new { b.SchoolId, b.CreatedAt });

        builder.ToTable("Broadcasts");
    }
}
