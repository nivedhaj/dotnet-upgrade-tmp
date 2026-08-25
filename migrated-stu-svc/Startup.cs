using Npgsql;
﻿using Autofac;
using ct._Core.Policy;
using ct._Core.Persistence;
using ct._Core.Repo.Resource;
using ct._Core.Security;
using ct._Core.Service;
using ct._Core.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NLog.Config;
using NLog.Targets;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ct
{
    /// <summary>
    /// Startup class
    /// </summary>
    public class Startup
    {
        private readonly string api_version;
        private readonly string api_title;
        /// <summary>
        /// Configuration (read from settings)
        /// </summary>
        public IConfiguration Configuration;
        /// <summary>
        /// Statrt up Ctor
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            ConfigurationItemFactory.Default.CreateInstance = (Type type) =>
            {
                if (type == typeof(NLogRmqService))
                {
                    var _rmqService = new RabbitMQService(Configuration);
                    return new NLogRmqService(_rmqService);
                }
                else
                    return Activator.CreateInstance(type); //default
            };

            Target.Register<NLogRmqService>("RMQ");

            api_version = Configuration["Swagger:Version"];
            api_title = Configuration["Swagger:Title"];
        }

        ///   <summary>
        /// Configure services
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            
            var asm = Assembly.GetExecutingAssembly();
            //var filePath = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, $"{asm.GetName().Name}.xml");
            services.AddControllers().AddNewtonsoftJson();
            services.AddCors(options =>
            {
                options.AddPolicy(_Constants.CORS_POLICY,
                builder => builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithExposedHeaders("Authorization", "AccessToken", "CT-Context"));
                //.AllowCredentials());
            });
            services.AddMvc(options => options.OutputFormatters.Add(new HtmlOutputFormatter()));
            services.AddSingleton(Configuration);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<PostgresDocumentStore>();
            var postgresConnectionString = Configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException("ConnectionStrings:Postgres is required.");
            services.AddSingleton(NpgsqlDataSource.Create(postgresConnectionString));
            services.AddScoped<ct.Modules.Students.Infrastructure.Queries.StudentQueries>();
            services.AddScoped<ct.Modules.Students.Infrastructure.Commands.StudentCommands>();
            services.AddScoped<IPostgresSession>(ctx => ctx.GetRequiredService<PostgresDocumentStore>().OpenSession());
            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = api_title, Version = api_version });
                // c.IncludeXmlComments(filePath);
            });

            //Redis cache config
            RedisCache.Url = Configuration["Redis:Url"];
            RedisCache.Port = Convert.ToInt16(Configuration["Redis:Port"]);
            RedisCache.Password = Configuration["Redis:Password"];
        }

        /// <summary>
        /// Configure.
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="loggerFactory"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.ApplicationServices.GetRequiredService<PostgresDocumentStore>().EnsureSchema();
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            app.UseCors(_Constants.CORS_POLICY);
            app.Use_ExceptionHandlerMiddleware();
            app.Use_AuthMiddleware();
            var swagger_enabled = Configuration["Swagger:Enabled"];
            if (swagger_enabled == "true")
            {
                // Enable middleware to serve generated Swagger as a JSON endpoint.
                app.UseSwagger(c =>
                {
                    c.RouteTemplate = "api/stu/swagger/{documentname}/swagger.json";
                });
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"/api/stu/swagger/{api_version}/swagger.json", $"{api_title} {api_version}");
                    c.RoutePrefix = "api/stu/swagger";
                });
            }
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
        private void EnsurePostgresSchema(IServiceProvider services)
        {
            services.GetRequiredService<PostgresDocumentStore>().EnsureSchema();
        }

        /// <summary>
        /// ConfigureContainer is where you can register things directly
        /// with Autofac. This runs after ConfigureServices so the things
        /// here will override registrations made in ConfigureServices.
        /// Don't build the container; that gets done for you. If you
        /// need a reference to the container, you need to use the
        /// "Without ConfigureContainer" mechanism shown later.
        /// </summary>
        /// <param name="builder"></param>
        public void ConfigureContainer(ContainerBuilder builder)
        {
            var asm = Assembly.GetExecutingAssembly();
            // Console.WriteLine(asm.FullName);
            var ts = asm.GetExportedTypes().ToList().Where(t => t.Name.EndsWith("Cmd") || t.Name.EndsWith("Qry") || t.Name.EndsWith("Service")).Select(o => o.FullName).OrderBy(o => o).ToList();
            ts.ForEach(t => Console.WriteLine(t));
            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>();
            builder.RegisterType<UserContextProvider>().As<IUserContextProvider>();
            builder.RegisterType<UserContext>().As<IUserContext>();

            builder.RegisterAssemblyTypes(asm)
                   .Where(t => t.Name.EndsWith("Cmd") || t.Name.EndsWith("Qry") || t.Name.EndsWith("Service"))
                   .AsSelf();
            //register Policy Engines
            builder.RegisterType<DefaultPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.Default);
            builder.RegisterType<BTLCOLPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.BTLCOL);
            builder.RegisterType<VVSGCSPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.VVSGCS);
            builder.RegisterType<VVSPHSPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.VVSPHS);
            builder.RegisterType<VVSSPUPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.VVSSPU);
            builder.RegisterType<VVSPUCPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.VVSPUC);
            builder.RegisterType<AKSHFNPolicyEngine>().Keyed<IPolicyEngine>(PolicyEngineEnum.AKSHFN);
            builder.RegisterType<PolicyEngineProvider>().As<IPolicyEngineProvider>();
        }
    }
}