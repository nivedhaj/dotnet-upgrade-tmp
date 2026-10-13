using Ct.Shared.Abstractions;
using Ct.Shared.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Ct.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCtShared(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, HeaderUserContext>();
        return services;
    }
}
