using Ct.Infrastructure.Persistence;
using Ct.Modules.Academics;
using Ct.Modules.Fees;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ct.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "stu")));

        services.AddAcademicsModule();
        services.AddFeesModule();

        return services;
    }
}
