using EduVoice.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduVoice.Infrastructure.Data.Configurations;

public class CallCampaignConfiguration : IEntityTypeConfiguration<CallCampaign>
{
    public void Configure(EntityTypeBuilder<CallCampaign> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        builder.Property(c => c.FilterClass)
            .HasMaxLength(50);

        builder.Property(c => c.FilterSection)
            .HasMaxLength(10);

        builder.Property(c => c.CustomMessage)
            .HasMaxLength(2000);

        builder.Property(c => c.Type)
            .HasConversion<string>();

        builder.Property(c => c.Status)
            .HasConversion<string>();

        builder.Property(c => c.FilterFeesStatus)
            .HasConversion<string>();

        builder.HasIndex(c => c.SchoolId);
        builder.HasIndex(c => new { c.SchoolId, c.Status });

        builder.HasMany(c => c.Calls)
            .WithOne(call => call.Campaign)
            .HasForeignKey(call => call.CampaignId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable("Campaigns");
    }
}
