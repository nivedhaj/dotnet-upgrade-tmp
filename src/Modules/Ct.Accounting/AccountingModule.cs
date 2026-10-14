using Ct.Accounting.Application;
using Ct.Accounting.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ct.Accounting;

public static class AccountingModule
{
    public static IServiceCollection AddAccountingModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IAccountingUserContext, HeaderAccountingUserContext>();
        services.AddScoped<LedgerService>();
        services.AddScoped<VoucherService>();
        services.AddScoped<InventoryService>();
        services.AddScoped<SmsCreditService>();
        services.AddScoped<AccountingReportService>();
        services.AddScoped<AccountingCmd>();
        services.AddScoped<AccountingQry>();

        services.AddDbContext<AccountingDbContext>(options =>
            options.UseNpgsql(
                config.GetConnectionString("Postgres")
                ?? config.GetConnectionString("DefaultConnection")
                ?? "Host=localhost;Database=ct_modular;Username=ct;Password=ct_dev_password"));

        return services;
    }

    public static WebApplication MapAccountingEndpoints(this WebApplication app) => app;
}
