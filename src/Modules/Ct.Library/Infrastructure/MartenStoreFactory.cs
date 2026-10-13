using Ct.Library.Postgres.ReadModels;
using Marten;
using Marten.Events.Projections;
using Weasel.Core;

namespace Ct.Library.Postgres.Infrastructure;

public static class MartenStoreFactory
{
    public static void ConfigureMarten(StoreOptions opts, IConfiguration configuration, IHostEnvironment env)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' is required.");

        opts.Connection(connectionString);

        // Use System.Text.Json by default; switch to Newtonsoft if needed for legacy types
        opts.UseSystemTextJsonForSerialization(enumStorage: EnumStorage.AsString);

        // Schema ownership – production should use a dedicated role
        if (env.IsDevelopment())
        {
            opts.AutoCreateSchemaObjects = AutoCreate.All;
        }
        else
        {
            opts.AutoCreateSchemaObjects = AutoCreate.None; // apply migrations explicitly
        }

        // Document identity – keep string GUIDs to match original Raven Ids
        opts.Schema.For<MaterialView>()
            .Identity(x => x.Id)
            .Index(x => x.TrackingId)
            .Index(x => x.Title)
            .Index(x => x.ISBN)
            .Index(x => x.OwnerId)
            .GinIndexJsonb(); // useful for attribute/tag queries

        // Register projections
        opts.Projections.Add<MaterialProjection>(ProjectionLifecycle.Inline);

        // Optional: multi-tenancy policy (explicit InstId still preferred for compatibility)
        // opts.Policies.AllDocumentsAreMultiTenanted();

        // Event store settings
        opts.Events.StreamIdentity = StreamIdentity.AsString; // AggregateId is string
        opts.Events.MetadataConfig.EnableAll();
    }
}
