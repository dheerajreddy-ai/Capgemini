using EduVoice.API.Middleware;
using EduVoice.Infrastructure.Data;
using EduVoice.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;

namespace EduVoice.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseEduVoiceMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        return app;
    }

    public static IApplicationBuilder UseTenantContext(this IApplicationBuilder app)
    {
        app.UseMiddleware<TenantMiddleware>();
        return app;
    }

    public static async Task<IApplicationBuilder> InitializeDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            var dbContext = services.GetRequiredService<AppDbContext>();

            logger.LogInformation("Running database migrations...");
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrations completed");

            logger.LogInformation("Running database seeder...");
            await DbSeeder.SeedAsync(dbContext, logger);
            logger.LogInformation("Database seeding completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during database initialization");
            throw;
        }

        return app;
    }
}
