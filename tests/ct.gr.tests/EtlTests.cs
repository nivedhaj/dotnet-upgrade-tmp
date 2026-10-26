using System.Text.Json;
using Ct.Gr.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Gr.Tests;

/// <summary>
/// Port of EtlTests. Raven ETL scripts are replaced by import via CreateAsync / bulk seed.
/// </summary>
[TestClass]
public class EtlTests : TestBase
{
    [TestMethod]
    public void Can_read_json_from_file()
    {
        var json = """
            [
              { "customerName": "A", "amount": 100, "particular": "Fee" },
              { "customerName": "B", "amount": 250, "particular": "Donation" }
            ]
            """;
        var rows = JsonSerializer.Deserialize<List<EtlReceiptRow>>(json);
        Assert.IsNotNull(rows);
        Assert.AreEqual(2, rows!.Count);
        Assert.AreEqual(100m, rows[0].Amount);
    }

    [TestMethod]
    public async Task Can_import_receipts_from_json_payload()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var rows = new[]
            {
                new EtlReceiptRow("Auro Student", 500m, "Tuition"),
                new EtlReceiptRow("Donor", 1000m, "Donation")
            };
            foreach (var row in rows)
            {
                var req = SampleReceipt(row.CustomerName, row.Amount, row.Particular,
                    receiptType: row.Particular.Equals("Donation", StringComparison.OrdinalIgnoreCase) ? "Donation" : "Regular");
                await svc.CreateAsync(req);
            }
            var page = await svc.GetAsync(null, null, null, 1, 20);
            Assert.AreEqual(2, page.Total);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_receipts_for_owner_after_etl_seed()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            for (var i = 0; i < 3; i++)
                await svc.CreateAsync(SampleReceipt($"Stu {i}", 100m * (i + 1)));
            var page = await svc.GetAsync(DateTime.UtcNow.Date.AddDays(-1), DateTime.UtcNow.Date.AddDays(1), "Active", 1, 10);
            Assert.IsTrue(page.Total >= 3);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    private sealed record EtlReceiptRow(string CustomerName, decimal Amount, string Particular);
}
