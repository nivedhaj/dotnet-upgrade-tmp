using Ct.Modules.Academics.Application.Students;
using Ct.Modules.Academics.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Ct.Modules.Academics;

public static class AcademicsModule
{
    public static IServiceCollection AddAcademicsModule(this IServiceCollection services)
    {
        services.AddScoped<StudentService>();
        services.AddScoped<IStudentQueries>(sp => sp.GetRequiredService<StudentService>());
        services.AddScoped<IStudentCommands>(sp => sp.GetRequiredService<StudentService>());
        return services;
    }
}
