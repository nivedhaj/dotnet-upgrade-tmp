using Ct.Fee.Application.Integrations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Fee.Tests;

/// <summary>
/// Port of EtlTests / EtlJsonlTests.
/// Modular fee no longer embeds Raven ETL scripts; coverage is the publish surface
/// used by the host to enqueue ETL worker jobs (LoggingFeeEventPublisher in tests).
/// </summary>
[TestClass]
public class EtlTests : TestBase
{
    [TestMethod]
    public async Task Can_publish_fee_structure_etl_job()
    {
        var events = CreateEvents();
        await events.PublishEtlScriptAsync(new FeeJobMessage
        {
            id = Guid.NewGuid().ToString(),
            title = "fee_structure_etl",
            payload = new { ownerId = Guid.NewGuid(), action = "export_structure" }
        });
    }

    [TestMethod]
    public async Task Can_publish_fee_txns_etl_job()
    {
        var events = CreateEvents();
        await events.PublishEtlScriptAsync(new FeeJobMessage
        {
            id = Guid.NewGuid().ToString(),
            title = "fee_txns_etl",
            payload = new { ownerId = Guid.NewGuid(), action = "export_transactions" }
        });
    }

    [TestMethod]
    public async Task Can_publish_import_fees_to_new_institute_etl()
    {
        var events = CreateEvents();
        await events.PublishEtlScriptAsync(new FeeJobMessage
        {
            id = Guid.NewGuid().ToString(),
            title = "import_fees_new_institute",
            payload = new
            {
                sourceOwnerId = Guid.NewGuid(),
                targetOwnerId = Guid.NewGuid(),
                includeTransactions = true
            }
        });
        await events.PublishJobAsync(new FeeJobMessage
        {
            id = Guid.NewGuid().ToString(),
            title = "fee_migration",
            payload = new { note = "queued" }
        });
    }
}
