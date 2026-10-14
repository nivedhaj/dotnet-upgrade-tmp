using Ct.Admission.Application;
using Ct.Admission.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ct.Admission;

public static class AdmissionModule
{
    public static IServiceCollection AddAdmissionModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IAdmissionUserContext, HeaderAdmissionUserContext>();
        services.AddScoped<ApplicationService>();
        services.AddScoped<AdmissionService>();
        services.AddScoped<TemplateService>();
        services.AddScoped<OtpService>();
        services.AddScoped<AdmissionCmd>();
        services.AddScoped<AdmissionQry>();

        services.AddDbContext<AdmissionDbContext>(options =>
            options.UseNpgsql(
                config.GetConnectionString("Postgres")
                ?? config.GetConnectionString("DefaultConnection")
                ?? "Host=localhost;Database=ct_modular;Username=ct;Password=ct_dev_password"));

        return services;
    }

    public static WebApplication MapAdmissionEndpoints(this WebApplication app) => app;
}
