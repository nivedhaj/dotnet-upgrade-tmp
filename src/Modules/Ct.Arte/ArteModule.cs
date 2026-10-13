using Ct.Arte.Application.Services;
using Ct.Arte.Infrastructure.Persistence;
using Ct.Arte.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ct.Arte;

public static class ArteModule
{
    public static IServiceCollection AddArteModule(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Postgres")
            ?? config.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=ct_modular;Username=ct;Password=ct_dev_password";

        services.AddScoped<ICurrentUser>(sp =>
        {
            var http = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
            return new CurrentUser
            {
                UserId = http?.User?.FindFirst("sub")?.Value
                         ?? http?.Request.Headers["X-User-Id"].FirstOrDefault()
                         ?? "system",
                OwnerId = http?.Request.Headers["X-Owner-Id"].FirstOrDefault()
                          ?? http?.User?.FindFirst("owner_id")?.Value
                          ?? "default",
                Email = http?.User?.FindFirst("email")?.Value,
                IsRootAdmin = string.Equals(http?.Request.Headers["X-Root-Admin"], "true", StringComparison.OrdinalIgnoreCase)
            };
        });

        services.AddScoped<AuditInterceptor>();

        services.AddDbContextPool<ArtefactDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(ArtefactDbContext).Assembly.FullName);
                npgsql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null);
            });
            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        services.AddScoped<IArtefactRepository, ArtefactRepository>();
        services.AddScoped<IArtefactService, ArtefactService>();
        return services;
    }

    public static WebApplication MapArteEndpoints(this WebApplication app)
    {
        return app;
    }
}
