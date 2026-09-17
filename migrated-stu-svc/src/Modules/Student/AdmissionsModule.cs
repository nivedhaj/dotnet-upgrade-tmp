using Ct.Modules.Admissions.Application.Commands;
using Ct.Modules.Admissions.Application.Queries;

namespace Ct.Modules.Admissions;

public static class AdmissionsModule
{
    public static IServiceCollection AddAdmissionsModule(this IServiceCollection services)
    {
        services.AddScoped<IStudentCommandService, StudentCommandService>();
        services.AddScoped<IStudentQueryService, StudentQueryService>();
        return services;
    }
}
