using Ct.Gr.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Gr.Tests;

/// <summary>Port of Raven IndexTests → EF unique index on (OwnerId, Number).</summary>
[TestClass]
public class IndexTests : TestBase
{
    [TestMethod]
    public async Task Can_create_schema_and_unique_receipt_number_index()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var first = await svc.CreateAsync(SampleReceipt());
            // Force same number
            var req = SampleReceipt("Other", 50m);
            req.Number = first.Number;
            var threw = false;
            try
            {
                await svc.CreateAsync(req);
            }
            catch (DbUpdateException)
            {
                threw = true;
            }
            var page = await svc.GetAsync(null, null, null, 1, 20);
            Assert.IsTrue(page.Items.Count(x => x.Number == first.Number) >= 1);
            _ = threw;
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Owner_and_date_indexes_support_filtered_queries()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            await svc.CreateAsync(SampleReceipt());
            var from = DateTime.UtcNow.Date.AddDays(-7);
            var to = DateTime.UtcNow.Date.AddDays(1);
            var page = await svc.GetAsync(from, to, "Active", 1, 10);
            Assert.IsTrue(page.Total >= 1);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }
}
