using Ct.Library.Postgres.Infrastructure;
using Ct.Library.Postgres.Queries;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ct.Library;

public static class LibraryModule
{
    public static IServiceCollection AddLibraryModule(this IServiceCollection services, IConfiguration config)
    {
        // Marten document store
        services.AddMarten(opts =>
        {
            // Host environment is resolved later; use a lightweight factory path
            var connectionString = config.GetConnectionString("Postgres")
                ?? config.GetConnectionString("DefaultConnection")
                ?? "Host=localhost;Port=5432;Database=ct_modular;Username=ct;Password=ct_dev_password";
            opts.Connection(connectionString);
            opts.UseSystemTextJsonForSerialization(enumStorage: Marten.Services.EnumStorage.AsString);
            opts.AutoCreateSchemaObjects = Weasel.Core.AutoCreate.All; // dev default
            opts.Schema.For<Ct.Library.Postgres.ReadModels.MaterialView>()
                .Identity(x => x.Id)
                .Index(x => x.TrackingId)
                .Index(x => x.Title)
                .Index(x => x.ISBN)
                .Index(x => x.OwnerId)
                .GinIndexJsonb();
            opts.Projections.Add<Ct.Library.Postgres.ReadModels.MaterialProjection>(Marten.Events.Projections.ProjectionLifecycle.Inline);
        })
        .UseLightweightSessions();

        services.AddSingleton<IUserContextAccessor>(sp =>
        {
            var accessor = sp.GetService<IHttpContextAccessor>();
            return new HttpUserContextAccessor(accessor!);
        });
        services.AddSingleton<IEventBus, InProcessEventBus>();
        services.AddScoped<IEventStore, MartenEventStore>();
        services.AddScoped<MaterialQuery>();
        return services;
    }

    public static WebApplication MapLibraryEndpoints(this WebApplication app)
    {
        return app;
    }
}

// Local adapter so the module does not depend on the original Program.cs file-scoped type
file sealed class HttpUserContextAccessor(IHttpContextAccessor? accessor) : IUserContextAccessor
{
    public UserContext Current
    {
        get
        {
            var ctx = accessor?.HttpContext;
            var userId = ctx?.Request.Headers["X-User-Id"].FirstOrDefault()
                         ?? ctx?.User?.FindFirst("sub")?.Value
                         ?? "system";
            var instId = ctx?.Request.Headers["X-Owner-Id"].FirstOrDefault()
                         ?? ctx?.Request.Headers["X-Inst-Id"].FirstOrDefault()
                         ?? "default";
            return new UserContext(userId, instId);
        }
    }
}
