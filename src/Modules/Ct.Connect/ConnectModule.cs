using Ct.Connect.Application;
using Ct.Connect.Infrastructure.Persistence;
using Ct.Connect.Infrastructure.Repositories;
using Ct.Connect.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ct.Connect;

public static class ConnectModule
{
    public static IServiceCollection AddConnectModule(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Postgres")
            ?? config.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=ct_modular;Username=ct;Password=ct_dev_password";

        services.AddHttpContextAccessor();
        services.AddHttpClient("ConnectEvents");
        services.AddHttpClient("MsGraph");
        services.AddHttpClient("Stu");

        services.AddScoped<AuditInterceptor>();
        services.AddScoped<IUserContextAccessor, HttpUserContextAccessor>();
        services.AddScoped<ConnectCmd>();
        services.AddScoped<ConnectQry>();
        services.AddScoped<TopicCmd>();
        services.AddScoped<WorkflowService>();
        services.AddScoped<IMsAuthService, MsAuthService>();
        services.AddScoped<IMsTeamService, MsTeamService>();

        services.Configure<ConnectRabbitMqOptions>(o =>
        {
            var s = config.GetSection(ConnectRabbitMqOptions.SectionName);
            if (s.Exists()) s.Bind(o);
            else
            {
                // Fall back to Fee:RabbitMq / RabbitMq
                o.Enabled = config.GetValue("Connect:RabbitMq:Enabled", false)
                            || config.GetValue("Fee:RabbitMq:Enabled", false)
                            || config.GetValue("RabbitMq:Enabled", false);
                o.HostName = config["Connect:RabbitMq:HostName"] ?? config["Fee:RabbitMq:HostName"] ?? config["RabbitMq:HostName"] ?? o.HostName;
                o.VirtualHost = config["Connect:RabbitMq:VirtualHost"] ?? config["Fee:RabbitMq:VirtualHost"] ?? o.VirtualHost;
                o.UserName = config["Connect:RabbitMq:UserName"] ?? config["Fee:RabbitMq:UserName"] ?? o.UserName;
                o.Password = config["Connect:RabbitMq:Password"] ?? config["Fee:RabbitMq:Password"] ?? o.Password;
                o.Port = config.GetValue("Connect:RabbitMq:Port", config.GetValue("Fee:RabbitMq:Port", o.Port));
            }
        });

        var rmqEnabled = config.GetValue("Connect:RabbitMq:Enabled", false)
                         || config.GetValue("Fee:RabbitMq:Enabled", false)
                         || config.GetValue("RabbitMq:Enabled", false);
        if (rmqEnabled)
            services.AddSingleton<IConnectEventPublisher, RabbitMqConnectEventPublisher>();
        else
            services.AddSingleton<IConnectEventPublisher, LoggingConnectEventPublisher>();

        services.AddDbContext<ConnectDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                npgsql.CommandTimeout(30);
            });
            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        return services;
    }

    public static WebApplication MapConnectEndpoints(this WebApplication app) => app;
}
