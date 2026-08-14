using Autofac;
using ct._Core.Policy;
using ct._Core.Security;
using ct._Core.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Student;
using System.Linq;

namespace Ct.Host
{
    /// <summary>
    /// The one and only executable/deployable in the new structure. Replaces every
    /// microservice's own Startup.cs — each module contributes its controllers via
    /// AddApplicationPart and its DI registrations via its own *Module.Register(...) call.
    ///
    /// Wired for the Student module only for now — this is deliberately the "stand up the
    /// host with one module first, validate DI/routing/Swagger end-to-end" step from the
    /// migration plan (target-modular-structure.md, §4, step 3) before Fees is added.
    /// Adding Fees later means: reference Fees.csproj, uncomment the two lines marked below.
    /// Nothing else in this file changes per module.
    /// </summary>
    public class Startup
    {
        public IConfiguration Configuration;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                    .AddNewtonsoftJson()
                    // Every module's controllers live in that module's own assembly now,
                    // instead of "whatever the current microservice's executing assembly is".
                    .AddApplicationPart(typeof(Student.Controllers.StudentController).Assembly);
                    // .AddApplicationPart(typeof(Fees.Controllers.FeesController).Assembly);

            services.AddCors(options =>
            {
                options.AddPolicy(_Constants.CORS_POLICY,
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("Authorization", "AccessToken", "CT-Context"));
            });

            services.AddSingleton(Configuration);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSwaggerGen(c =>
            {
                // One Swagger doc for the whole host. Individual modules no longer mount
                // their own swagger.json under a service-specific route (e.g. api/stu/swagger)
                // — everything is one API surface now, grouped by controller route prefix
                // (api/student/..., api/fees/...) instead of by deployable.
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Campus API", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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

        /// <summary>
        /// Wires every module's DI registrations into the one shared container. This is the
        /// direct replacement for the old per-microservice ConfigureContainer — the shared
        /// platform bits (policy engines, auth, user-context plumbing) still register once,
        /// here; everything module-specific delegates to that module's own *Module class.
        /// </summary>
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>();

            // Shared policy engines — unchanged from the original Startup.cs, these are
            // genuinely cross-cutting (per-institute business rules), not owned by one module.
            builder.RegisterType<DefaultPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.Default);
            builder.RegisterType<BTLCOLPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.BTLCOL);
            builder.RegisterType<VVSGCSPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.VVSGCS);
            builder.RegisterType<VVSPHSPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.VVSPHS);
            builder.RegisterType<VVSSPUPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.VVSSPU);
            builder.RegisterType<VVSPUCPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.VVSPUC);
            builder.RegisterType<AKSHFNPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.AKSHFN);
            builder.RegisterType<PolicyEngineProvider>().As<IPolicyEngineProvider>();

            // Each module owns its own database, session, and DI registrations. Adding a
            // module = adding one line here (plus the AddApplicationPart line above).
            StudentModule.Register(builder, Configuration);
            // FeesModule.Register(builder, Configuration);   // next module to bring in
            // StaffModule.Register(builder, Configuration);
        }
    }
}
