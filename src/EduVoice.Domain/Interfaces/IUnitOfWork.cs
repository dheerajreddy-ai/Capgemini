using EduVoice.Domain.Entities;

namespace EduVoice.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<School> Schools { get; }
    IRepository<User> Users { get; }
    IRepository<Student> Students { get; }
    IRepository<CallCampaign> Campaigns { get; }
    IRepository<Call> Calls { get; }
    IRepository<Complaint> Complaints { get; }
    IRepository<AuditLog> AuditLogs { get; }
    IRepository<Broadcast> Broadcasts { get; }
    IRepository<ExamSchedule> ExamSchedules { get; }
    IRepository<Homework> Homeworks { get; }
    IRepository<FeeInstalment> FeeInstalments { get; }
    IRepository<StaffAbsence> StaffAbsences { get; }
    IRepository<PtmSchedule> PtmSchedules { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
