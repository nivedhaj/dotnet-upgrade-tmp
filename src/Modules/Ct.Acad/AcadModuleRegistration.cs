using Ct.Acad.Application;
using Ct.Acad.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ct.Acad;

public static class AcadModule
{
    public static IServiceCollection AddAcadModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IAcadUserContext, HeaderAcadUserContext>();
        services.AddScoped<CourseService>();
        services.AddScoped<ExamService>();
        services.AddScoped<AssessmentService>();
        services.AddScoped<AttendanceService>();
        services.AddScoped<GradingService>();
        services.AddScoped<LmsService>();
        services.AddScoped<TtService>();
        services.AddScoped<CalendarService>();
        services.AddScoped<AcadReportService>();
        services.AddScoped<CalendarExtrasService>();
        services.AddScoped<ArtefactService>();
        services.AddScoped<ReportExtrasService>();
        services.AddScoped<ExamExtrasService>();
        services.AddScoped<AssessmentExtrasService>();
        services.AddScoped<CourseExtrasService>();

        services.AddDbContext<AcadDbContext>(options =>
            options.UseNpgsql(
                config.GetConnectionString("Postgres")
                ?? config.GetConnectionString("DefaultConnection")
                ?? "Host=localhost;Database=ct_modular;Username=ct;Password=ct_dev_password"));
        return services;
    }

    public static WebApplication MapAcadEndpoints(this WebApplication app) => app;
}
