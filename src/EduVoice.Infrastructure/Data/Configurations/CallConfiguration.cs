using EduVoice.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduVoice.Infrastructure.Data.Configurations;

public class CallConfiguration : IEntityTypeConfiguration<Call>
{
    public void Configure(EntityTypeBuilder<Call> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.VapiCallId)
            .HasMaxLength(200);

        builder.HasIndex(c => c.VapiCallId);

        builder.Property(c => c.ToPhone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.FromPhone)
            .HasMaxLength(20);

        builder.Property(c => c.RecordingUrl)
            .HasMaxLength(500);

        builder.Property(c => c.ComplaintSummary)
            .HasMaxLength(2000);

        builder.Property(c => c.AiSummary)
            .HasMaxLength(2000);

        builder.Property(c => c.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(c => c.Type)
            .HasConversion<string>();

        builder.Property(c => c.Status)
            .HasConversion<string>();

        builder.Property(c => c.Direction)
            .HasConversion<string>();

        builder.Property(c => c.Sentiment)
            .HasConversion<string>();

        builder.Property(c => c.Language)
            .HasConversion<string>();

        builder.Property(c => c.EscalationReason)
            .HasMaxLength(500);

        builder.Property(c => c.DialectUsed)
            .HasMaxLength(50);

        builder.Property(c => c.NetworkQuality)
            .HasMaxLength(20);

        builder.HasIndex(c => c.SchoolId);
        builder.HasIndex(c => c.StudentId);
        builder.HasIndex(c => c.CampaignId);
        builder.HasIndex(c => new { c.SchoolId, c.CreatedAt });
        builder.HasIndex(c => new { c.SchoolId, c.Status });

        builder.HasOne(c => c.Student)
            .WithMany(s => s.Calls)
            .HasForeignKey(c => c.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Complaints)
            .WithOne(comp => comp.Call)
            .HasForeignKey(comp => comp.CallId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable("Calls");
    }
}
