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
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
