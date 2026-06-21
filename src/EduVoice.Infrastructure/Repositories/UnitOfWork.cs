using EduVoice.Domain.Entities;
using EduVoice.Domain.Interfaces;
using EduVoice.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace EduVoice.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    private IRepository<School>? _schools;
    private IRepository<User>? _users;
    private IRepository<Student>? _students;
    private IRepository<CallCampaign>? _campaigns;
    private IRepository<Call>? _calls;
    private IRepository<Complaint>? _complaints;
    private IRepository<AuditLog>? _auditLogs;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepository<School> Schools => _schools ??= new Repository<School>(_context);
    public IRepository<User> Users => _users ??= new Repository<User>(_context);
    public IRepository<Student> Students => _students ??= new Repository<Student>(_context);
    public IRepository<CallCampaign> Campaigns => _campaigns ??= new Repository<CallCampaign>(_context);
    public IRepository<Call> Calls => _calls ??= new Repository<Call>(_context);
    public IRepository<Complaint> Complaints => _complaints ??= new Repository<Complaint>(_context);
    public IRepository<AuditLog> AuditLogs => _auditLogs ??= new Repository<AuditLog>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
