using Ct.Gr.Application;
using Ct.Gr.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ct.Gr;

public static class GrModule
{
    public static IServiceCollection AddGrModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IGrUserContext, HeaderGrUserContext>();
        services.AddScoped<IGrPolicy, ConfigGrPolicy>();
        services.AddScoped<IReceiptPrintService, ReceiptPrintService>();
        services.AddScoped<ReceiptService>();

        services.Configure<GrRabbitMqOptions>(o =>
        {
            var s = config.GetSection(GrRabbitMqOptions.SectionName);
            if (s.Exists()) s.Bind(o);
            else
            {
                o.Enabled = config.GetValue("Gr:RabbitMq:Enabled", false)
                            || config.GetValue("Fee:RabbitMq:Enabled", false)
                            || config.GetValue("RabbitMq:Enabled", false);
                o.HostName = config["Gr:RabbitMq:HostName"] ?? config["Fee:RabbitMq:HostName"] ?? config["RabbitMq:HostName"] ?? o.HostName;
                o.VirtualHost = config["Gr:RabbitMq:VirtualHost"] ?? config["Fee:RabbitMq:VirtualHost"] ?? o.VirtualHost;
                o.UserName = config["Gr:RabbitMq:UserName"] ?? config["Fee:RabbitMq:UserName"] ?? o.UserName;
                o.Password = config["Gr:RabbitMq:Password"] ?? config["Fee:RabbitMq:Password"] ?? o.Password;
                o.Port = config.GetValue("Gr:RabbitMq:Port", config.GetValue("Fee:RabbitMq:Port", o.Port));
            }
        });

        var rmq = config.GetValue("Gr:RabbitMq:Enabled", false)
                  || config.GetValue("Fee:RabbitMq:Enabled", false)
                  || config.GetValue("RabbitMq:Enabled", false);
        if (rmq)
            services.AddSingleton<IGrEventPublisher, RabbitMqGrEventPublisher>();
        else
            services.AddSingleton<IGrEventPublisher, LoggingGrEventPublisher>();

        services.AddDbContext<GrDbContext>(options =>
            options.UseNpgsql(
                config.GetConnectionString("Postgres")
                ?? config.GetConnectionString("DefaultConnection")
                ?? "Host=localhost;Database=ct_modular;Username=ct;Password=ct_dev_password"));
        return services;
    }

    public static WebApplication MapGrEndpoints(this WebApplication app) => app;
}
