using Ct.Core.Application;
using Ct.Core.Infrastructure;
using Ct.Modules.Admissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ct.Stu;

public static class StuModule
{
    public static IServiceCollection AddStuModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IUserContext, HttpUserContext>();
        services.AddDbContext<CtDbContext>(options =>
            options.UseNpgsql(
                config.GetConnectionString("Postgres") ?? config.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "ct")));
        services.AddAdmissionsModule();
        return services;
    }

    public static WebApplication MapStuEndpoints(this WebApplication app)
    {
        // Controllers under Ct.Modules.Admissions.Api are discovered via MapControllers()
        return app;
    }
}
