using Ct.Fee.Application;
using Ct.Fee.Application.Integrations;
using Ct.Fee.Application.Policy;
using Ct.Fee.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ct.Fee;

public static class FeeModule
{
    public static IServiceCollection AddFeeModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpContextAccessor();
        services.AddHttpClient("Stu");
        services.AddHttpClient("FeeEvents");

        services.AddScoped<ICurrentUserContext, HeaderCurrentUserContext>();
        services.AddScoped<FeeCalculationService>();

        // Policy engines (Fee:PolicyEngine = Default | BTLCOL | VVSGCS | VVSPHS | VVSBMS)
        services.AddSingleton<IFeePolicyEngineProvider, FeePolicyEngineProvider>();
        services.AddScoped<IFeePolicyEngine>(sp =>
        {
            var provider = sp.GetRequiredService<IFeePolicyEngineProvider>();
            var policyId = config["Fee:PolicyEngine"] ?? config["FeeModule:PolicyEngine"];
            return provider.For(policyId);
        });

        // Student directory — HTTP when BaseUrl configured, else null enrichment
        var stuBase = config["Fee:StudentService:BaseUrl"] ?? config["Services:Stu:BaseUrl"];
        if (!string.IsNullOrWhiteSpace(stuBase))
            services.AddScoped<IStudentDirectory, HttpStudentDirectory>();
        else
            services.AddScoped<IStudentDirectory, NullStudentDirectory>();

        // Fee receivable callback to Stu
        if (!string.IsNullOrWhiteSpace(stuBase))
            services.AddScoped<IStudentFeeReceivableClient, HttpStudentFeeReceivableClient>();
        else
            services.AddScoped<IStudentFeeReceivableClient, NullStudentFeeReceivableClient>();

        // ── RabbitMQ / fee_tx_done message bus ─────────────────────────────────
        services.Configure<RabbitMqOptions>(config.GetSection(RabbitMqOptions.SectionName));
        // Also bind legacy flat keys if present
        services.PostConfigure<RabbitMqOptions>(o =>
        {
            if (config.GetSection(RabbitMqOptions.SectionName).Exists()) return;
            // Support Fee:RabbitMq:Host style already under section; also top-level RabbitMq:*
            o.Enabled = config.GetValue("Fee:RabbitMq:Enabled", o.Enabled)
                        || config.GetValue("RabbitMq:Enabled", false);
            o.HostName = config["Fee:RabbitMq:HostName"] ?? config["RabbitMq:HostName"] ?? o.HostName;
            o.VirtualHost = config["Fee:RabbitMq:VirtualHost"] ?? config["RabbitMq:VirtualHost"] ?? o.VirtualHost;
            o.UserName = config["Fee:RabbitMq:UserName"] ?? config["RabbitMq:UserName"] ?? o.UserName;
            o.Password = config["Fee:RabbitMq:Password"] ?? config["RabbitMq:Password"] ?? o.Password;
            o.Port = config.GetValue("Fee:RabbitMq:Port", config.GetValue("RabbitMq:Port", o.Port));
            o.FeeTopicExchange = config["Fee:RabbitMq:FeeTopicExchange"] ?? config["RabbitMq:FeeTopicExchange"] ?? o.FeeTopicExchange;
            o.FeeTxDoneRoutingKey = config["Fee:RabbitMq:FeeTxDoneRoutingKey"] ?? o.FeeTxDoneRoutingKey;
            o.FeeTxDoneQueue = config["Fee:RabbitMq:FeeTxDoneQueue"] ?? o.FeeTxDoneQueue;
        });

        var rmqEnabled = config.GetValue("Fee:RabbitMq:Enabled", false)
                         || config.GetValue("RabbitMq:Enabled", false);

        if (rmqEnabled)
        {
            services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
            services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
            services.AddScoped<RabbitMqFeeEventPublisher>();
            services.AddScoped<IFeeEventPublisher>(sp =>
                new ResilientFeeEventPublisher(
                    sp.GetRequiredService<RabbitMqFeeEventPublisher>(),
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ResilientFeeEventPublisher>>()));
        }
        else
        {
            services.AddScoped<IFeeEventPublisher, LoggingFeeEventPublisher>();
        }

        services.AddScoped<FeeService>();
        services.AddScoped<ReportService>();
        services.AddDbContext<FeeDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("Postgres") ?? config.GetConnectionString("DefaultConnection")));
        return services;
    }

    public static WebApplication MapFeeEndpoints(this WebApplication app)
    {
        return app;
    }
}
