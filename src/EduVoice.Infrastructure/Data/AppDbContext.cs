using EduVoice.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduVoice.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<School> Schools => Set<School>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<CallCampaign> Campaigns => Set<CallCampaign>();
    public DbSet<Call> Calls => Set<Call>();
    public DbSet<Complaint> Complaints => Set<Complaint>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Broadcast> Broadcasts => Set<Broadcast>();
    public DbSet<ExamSchedule> ExamSchedules => Set<ExamSchedule>();
    public DbSet<Homework> Homeworks => Set<Homework>();
    public DbSet<FeeInstalment> FeeInstalments => Set<FeeInstalment>();
    public DbSet<StaffAbsence> StaffAbsences => Set<StaffAbsence>();
    public DbSet<PtmSchedule> PtmSchedules => Set<PtmSchedule>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.GetType().GetProperty("CreatedAt") != null)
                {
                    var createdAt = (DateTime?)entry.CurrentValues["CreatedAt"];
                    if (createdAt == null || createdAt == DateTime.MinValue)
                        entry.CurrentValues["CreatedAt"] = now;
                }
                if (entry.Entity.GetType().GetProperty("UpdatedAt") != null)
                    entry.CurrentValues["UpdatedAt"] = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Entity.GetType().GetProperty("UpdatedAt") != null)
                    entry.CurrentValues["UpdatedAt"] = now;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
