using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EduVoice.Infrastructure.Data;

/// <summary>
/// Used only by EF Core tooling (dotnet ef migrations add/update).
/// The real connection string comes from DATABASE_URL env var at runtime.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=eduvoice_dev;Username=postgres;Password=postgres")
            .Options;

        return new AppDbContext(options);
    }
}
