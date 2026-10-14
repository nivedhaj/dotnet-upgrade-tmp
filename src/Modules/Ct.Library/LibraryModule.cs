using Ct.Library.Application;
using Ct.Library.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ct.Library;

public static class LibraryModule
{
    public static IServiceCollection AddLibraryModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ILibraryUserContext, HeaderLibraryUserContext>();
        services.AddScoped<MaterialService>();
        services.AddScoped<MemberService>();
        services.AddScoped<CirculationService>();
        services.AddScoped<LibraryReportService>();
        services.AddSingleton<BarcodeService>();

        services.AddDbContext<LibraryDbContext>(options =>
            options.UseNpgsql(
                config.GetConnectionString("Postgres")
                ?? config.GetConnectionString("DefaultConnection")
                ?? "Host=localhost;Database=ct_modular;Username=ct;Password=ct_dev_password"));

        return services;
    }

    public static WebApplication MapLibraryEndpoints(this WebApplication app) => app;
}
