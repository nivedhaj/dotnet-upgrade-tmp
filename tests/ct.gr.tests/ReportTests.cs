using Ct.Gr.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Gr.Tests;

/// <summary>Port of original ReportTests.</summary>
[TestClass]
public class ReportTests : TestBase
{
    [TestMethod]
    public async Task Can_get_receipt_report()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            await svc.CreateAsync(SampleReceipt());
            var page = await svc.GetReceiptReportsAsync(DefaultFilter());
            Assert.IsTrue(page.Total >= 1);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_receipt_report_summary()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var r = await svc.CreateAsync(SampleReceipt(amount: 1200m));
            await svc.CancelAsync((await svc.CreateAsync(SampleReceipt("X", 300m))).Id);

            var summary = await svc.GetSummaryAsync(DefaultFilter());
            Assert.IsTrue(summary.Count >= 1);
            Assert.IsTrue(summary.TotalAmount >= 1200m);
            Assert.IsTrue(summary.CancelledCount >= 1);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_receipt_consolidated_report()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            await svc.CreateAsync(SampleReceipt(paymentMode: "Cash"));
            await svc.CreateAsync(SampleReceipt("B", 500m, paymentMode: "UPI"));
            var report = await svc.GetConsolidatedReportAsync(DefaultFilter());
            Assert.IsNotNull(report);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_receipt_revenue_sharing_by_class_report()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            await svc.CreateAsync(SampleReceipt(studyingIn: "II", section: "B", amount: 800m));
            var rows = await svc.GetRevenueByClassAsync(DefaultFilter());
            Assert.IsTrue(rows.Count >= 1);
            Assert.IsTrue(rows.Any(r => r.ClassName == "II"));
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_receipt_revenue_sharing_by_person_report()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            await svc.CreateAsync(SampleReceipt("Teacher X", 400m, particular: "Workshop"));
            // by particular (revenue sharing) and by person
            var byParticular = await svc.GetRevenueSharingAsync(DefaultFilter());
            Assert.IsTrue(byParticular.Any(r => r.Particular == "Workshop"));

            var byPerson = await svc.GetRevenueByPersonAsync(DefaultFilter());
            Assert.IsTrue(byPerson.Count >= 1);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_receipt_print()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var created = await svc.CreateAsync(SampleReceipt());
            var print = await svc.PrintAsync(created.Id);
            Assert.IsNotNull(print);
            Assert.IsTrue(print!.Html.Contains("Receipt"));
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_receipt_report_grouped()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            await svc.CreateAsync(SampleReceipt());
            var groups = await svc.GetReceiptReportGroupedAsync(DefaultFilter());
            Assert.IsTrue(groups.Count >= 1);
            Assert.IsTrue(groups[0].Receipts.Count >= 1);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }
}
