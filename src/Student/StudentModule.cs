using Autofac;
using Autofac.Core;
using ct._Core.Security;
using ct._Core.Service;
using Marten;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Reflection;

namespace Student
{
    /// <summary>
    /// Student module DI — PostgreSQL via Marten (document store on Postgres).
    /// Replaces RavenDB DocumentStore. Controllers / Views / Cmd / Qry keep the same shapes.
    /// </summary>
    public class StudentModule
    {
        public const string DatabaseName = "student";

        public static void Register(ContainerBuilder builder, IConfiguration configuration)
        {
            var asm = Assembly.GetExecutingAssembly();
            var cs = configuration.GetConnectionString("Student")
                     ?? configuration["ConnectionStrings:Student"]
                     ?? throw new InvalidOperationException("Missing ConnectionStrings:Student");

            builder.Register(ctx => CreateStore(cs))
                   .Named<IDocumentStore>(DatabaseName)
                   .SingleInstance();

            builder.Register(ctx => new UserContextProvider(
                        ctx.ResolveNamed<IDocumentStore>(DatabaseName),
                        ctx.Resolve<IHttpContextAccessor>(),
                        configuration,
                        ctx.Resolve<AppService>()))
                   .Named<IUserContextProvider>(DatabaseName)
                   .InstancePerLifetimeScope();

            builder.Register(ctx => new UserContext())
                   .Named<IUserContext>(DatabaseName)
                   .InstancePerLifetimeScope();

            var namedUcParam = new ResolvedParameter(
                (pi, ctx) => pi.ParameterType == typeof(IUserContext),
                (pi, ctx) => ctx.ResolveNamed<IUserContext>(DatabaseName));

            builder.RegisterAssemblyTypes(asm)
                   .Where(t => t.Name.EndsWith("Cmd") || t.Name.EndsWith("Qry") || t.Name.EndsWith("Service"))
                   .WithParameter(namedUcParam)
                   .AsSelf();
        }

        static IDocumentStore CreateStore(string connectionString)
        {
            return DocumentStore.For(opts =>
            {
                opts.Connection(connectionString);
                opts.DatabaseSchemaName = DatabaseName;
                // Auto-create schema in Development; use migrations in production.
                opts.AutoCreateSchemaObjects = Weasel.Core.AutoCreate.CreateOrUpdate;
                // Register view document
                opts.Schema.For<Student.Views.StudentFeeSummary>()
                    .Identity(x => x.StudentId);
            });
        }
    }
}
