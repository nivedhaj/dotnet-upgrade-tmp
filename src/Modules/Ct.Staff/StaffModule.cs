using Ct.Staff.Application;
using Ct.Staff.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ct.Staff;

public static class StaffModule
{
    public static IServiceCollection AddStaffModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IStaffUserContext, HeaderStaffUserContext>();
        services.AddScoped<StaffService>();
        services.AddScoped<SalaryService>();
        services.AddScoped<StaffReportService>();

        services.AddDbContext<StaffDbContext>(options =>
            options.UseNpgsql(
                config.GetConnectionString("Postgres")
                ?? config.GetConnectionString("DefaultConnection")
                ?? "Host=localhost;Database=ct_modular;Username=ct;Password=ct_dev_password"));

        return services;
    }

    public static WebApplication MapStaffEndpoints(this WebApplication app) => app;
}
