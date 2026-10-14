using Ct.Asset.Application;
using Ct.Asset.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ct.Asset;

public static class AssetModule
{
    public static IServiceCollection AddAssetModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IAssetUserContext, HeaderAssetUserContext>();
        services.AddScoped<AssetService>();
        services.AddScoped<AssetCmd>();
        services.AddScoped<AssetQry>();

        services.AddDbContext<AssetDbContext>(options =>
            options.UseNpgsql(
                config.GetConnectionString("Postgres")
                ?? config.GetConnectionString("DefaultConnection")
                ?? "Host=localhost;Database=ct_modular;Username=ct;Password=ct_dev_password"));

        return services;
    }

    public static WebApplication MapAssetEndpoints(this WebApplication app) => app;
}
