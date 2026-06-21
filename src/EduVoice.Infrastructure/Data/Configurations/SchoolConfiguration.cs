using EduVoice.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduVoice.Infrastructure.Data.Configurations;

public class SchoolConfiguration : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.SubDomain)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(s => s.SubDomain)
            .IsUnique();

        builder.Property(s => s.ContactEmail)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.ContactPhone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.LogoUrl)
            .HasMaxLength(500);

        builder.Property(s => s.PrimaryColor)
            .HasMaxLength(20);

        builder.Property(s => s.Address)
            .HasMaxLength(500);

        builder.Property(s => s.City)
            .HasMaxLength(100);

        builder.Property(s => s.State)
            .HasMaxLength(100);

        builder.Property(s => s.TwilioPhoneNumber)
            .HasMaxLength(20);

        builder.Property(s => s.VapiAssistantId)
            .HasMaxLength(200);

        builder.Property(s => s.ElevenLabsVoiceId)
            .HasMaxLength(200);

        builder.Property(s => s.PlanType)
            .HasConversion<string>();

        builder.HasMany(s => s.Users)
            .WithOne(u => u.School)
            .HasForeignKey(u => u.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Students)
            .WithOne(st => st.School)
            .HasForeignKey(st => st.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Campaigns)
            .WithOne(c => c.School)
            .HasForeignKey(c => c.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Calls)
            .WithOne(c => c.School)
            .HasForeignKey(c => c.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Complaints)
            .WithOne(c => c.School)
            .HasForeignKey(c => c.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("Schools");
    }
}
