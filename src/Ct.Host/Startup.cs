using Autofac;
using ct._Core.Data;
using ct._Core.Policy;
using ct._Core.Security;
using ct._Core.Util;
using Fees;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Student;

namespace Ct.Host;

public sealed class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration) => Configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers()
            .AddNewtonsoftJson()
            .AddApplicationPart(typeof(Student.Controllers.StudentController).Assembly)
            .AddApplicationPart(typeof(Fees.Controllers.FeesController).Assembly);

        services.AddCors(options =>
        {
            options.AddPolicy(_Constants.CORS_POLICY, builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithExposedHeaders("Authorization", "AccessToken", "CT-Context"));
        });

        services.AddHttpContextAccessor();
        services.AddSingleton(Configuration);

        services.AddSwaggerGen(c =>
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Campus API", Version = "v1" }));
    }

    public void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterType<PostgresSession>()
            .As<IPostgresSession>()
            .InstancePerLifetimeScope();

        builder.RegisterType<AppService>().SingleInstance();
        builder.RegisterType<UserContextProvider>()
            .As<IUserContextProvider>()
            .InstancePerLifetimeScope();
        builder.RegisterType<UserContext>()
            .As<IUserContext>()
            .InstancePerLifetimeScope();

        builder.RegisterType<DefaultPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.Default);
        builder.RegisterType<BTLCOLPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.BTLCOL);
        builder.RegisterType<VVSGCSPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.VVSGCS);
        builder.RegisterType<VVSPHSPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.VVSPHS);
        builder.RegisterType<VVSSPUPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.VVSSPU);
        builder.RegisterType<VVSPUCPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.VVSPUC);
        builder.RegisterType<AKSHFNPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.AKSHFN);
        builder.RegisterType<PolicyEngineProvider>().As<IPolicyEngineProvider>();

        StudentModule.Register(builder);
        FeesModule.Register(builder);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (Configuration.GetValue<bool>("Postgres:ApplySchemaOnStartup"))
        {
            var schemaPath = Path.Combine(env.ContentRootPath, "Database", "schema.sql");
            PostgresSchemaInitializer.Apply(Configuration, schemaPath);
        }

        app.UseCors(_Constants.CORS_POLICY);
        app.Use_ExceptionHandlerMiddleware();
        app.Use_AuthMiddleware();

        if (Configuration["Swagger:Enabled"] == "true")
        {
            app.UseSwagger(c => c.RouteTemplate = "api/swagger/{documentname}/swagger.json");
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "Campus API v1");
                c.RoutePrefix = "api/swagger";
            });
        }

        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
