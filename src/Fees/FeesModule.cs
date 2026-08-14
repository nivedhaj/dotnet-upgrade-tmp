using Autofac;
using Autofac.Core;
using Marten;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Reflection;

namespace Fees
{
    /// <summary>
    /// Fees module DI — PostgreSQL via Marten.
    /// </summary>
    public class FeesModule
    {
        public const string DatabaseName = "fees";

        public static void Register(ContainerBuilder builder, IConfiguration configuration)
        {
            var asm = Assembly.GetExecutingAssembly();
            var cs = configuration.GetConnectionString("Fees")
                     ?? configuration["ConnectionStrings:Fees"]
                     ?? throw new InvalidOperationException("Missing ConnectionStrings:Fees");

            builder.Register(ctx => CreateStore(cs))
                   .Named<IDocumentStore>(DatabaseName)
                   .SingleInstance();

            builder.Register(ctx => ctx.ResolveNamed<IDocumentStore>(DatabaseName).LightweightSession())
                   .Named<IDocumentSession>(DatabaseName)
                   .InstancePerLifetimeScope();

            var namedSessionParam = new ResolvedParameter(
                (pi, ctx) => pi.ParameterType == typeof(IDocumentSession),
                (pi, ctx) => ctx.ResolveNamed<IDocumentSession>(DatabaseName));

            builder.RegisterAssemblyTypes(asm)
                   .Where(t => t.Name.EndsWith("Cmd") || t.Name.EndsWith("Qry") || t.Name.EndsWith("Service"))
                   .WithParameter(namedSessionParam)
                   .AsSelf();
        }

        static IDocumentStore CreateStore(string connectionString)
        {
            return DocumentStore.For(opts =>
            {
                opts.Connection(connectionString);
                opts.DatabaseSchemaName = DatabaseName;
                opts.AutoCreateSchemaObjects = Weasel.Core.AutoCreate.CreateOrUpdate;
            });
        }
    }
}
