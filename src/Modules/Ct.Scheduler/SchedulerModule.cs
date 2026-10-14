using Ct.Scheduler.Application;
using Ct.Scheduler.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ct.Scheduler;

public static class SchedulerModule
{
    public static IServiceCollection AddSchedulerModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ISchedulerUserContext, HeaderSchedulerUserContext>();
        services.AddScoped<CalendarService>();
        services.AddScoped<SchedulerService>();
        services.AddScoped<SchedulerReportService>();
        services.AddScoped<CalendarCmd>();
        services.AddScoped<CalendarQry>();
        services.AddScoped<SchedulerReportCmd>();

        services.AddDbContext<SchedulerDbContext>(options =>
            options.UseNpgsql(
                config.GetConnectionString("Postgres")
                ?? config.GetConnectionString("DefaultConnection")
                ?? "Host=localhost;Database=ct_modular;Username=ct;Password=ct_dev_password"));

        return services;
    }

    public static WebApplication MapSchedulerEndpoints(this WebApplication app) => app;
}
