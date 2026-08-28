using Ct.Modules.Fees.Application;
using Ct.Modules.Fees.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Ct.Modules.Fees;

public static class FeesModule
{
    public static IServiceCollection AddFeesModule(this IServiceCollection services)
    {
        services.AddScoped<FeeQueryService>();
        services.AddScoped<IFeeQueries>(sp => sp.GetRequiredService<FeeQueryService>());
        return services;
    }
}
