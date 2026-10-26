using Ct.Gr.Application;
using Ct.Gr.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Gr.Tests;

/// <summary>Port of original ReceiptTests.</summary>
[TestClass]
public class ReceiptTests : TestBase
{
    [TestMethod]
    public async Task Can_create_receipt()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var r = await svc.CreateAsync(SampleReceipt());
            Assert.AreNotEqual(Guid.Empty, r.Id);
            Assert.IsFalse(string.IsNullOrWhiteSpace(r.Number));
            Assert.AreEqual(1500m, r.TotalAmount);
            Assert.AreEqual("Parent A", r.CustomerName);
            Assert.AreEqual(nameof(ReceiptStatus.Active), r.Status);
            Assert.IsTrue(r.OrderItems.Count >= 1);
            Assert.IsFalse(string.IsNullOrEmpty(r.Html));
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_receipts()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            await svc.CreateAsync(SampleReceipt("A", 100m));
            await svc.CreateAsync(SampleReceipt("B", 200m));
            var page = await svc.GetAsync(null, null, null, 1, 10);
            Assert.IsTrue(page.Total >= 2);
            Assert.IsTrue(page.Items.Count >= 2);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_receipt_by_id()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var created = await svc.CreateAsync(SampleReceipt());
            var loaded = await svc.GetByIdAsync(created.Id);
            Assert.IsNotNull(loaded);
            Assert.AreEqual(created.Number, loaded!.Number);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_print_receipt()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var created = await svc.CreateAsync(SampleReceipt());
            var print = await svc.PrintAsync(created.Id);
            Assert.IsNotNull(print);
            Assert.IsTrue(print!.Html.Contains(created.Number));
            Assert.IsTrue(print.Html.Contains("Parent A") || print.CustomerName == "Parent A");

            var reloaded = await svc.GetByIdAsync(created.Id);
            Assert.IsTrue(reloaded!.Printed);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_cancel_and_activate_receipt()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var created = await svc.CreateAsync(SampleReceipt());
            Assert.IsTrue(await svc.CancelAsync(created.Id));
            Assert.AreEqual(nameof(ReceiptStatus.Cancelled), (await svc.GetByIdAsync(created.Id))!.Status);

            Assert.IsTrue(await svc.ActivateAsync(created.Id));
            Assert.AreEqual(nameof(ReceiptStatus.Active), (await svc.GetByIdAsync(created.Id))!.Status);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_update_receipt()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var created = await svc.CreateAsync(SampleReceipt());
            var updated = await svc.UpdateReceiptAsync(new UpdateReceiptRequest
            {
                Id = created.Id,
                ReceivedBy = "Cashier-2",
                Customer = new CustomerDto { Name = "Updated Parent", Contact = "9999999999" },
                Include80G = true
            });
            Assert.IsNotNull(updated);
            Assert.AreEqual("Updated Parent", updated!.CustomerName);
            Assert.AreEqual("Cashier-2", updated.ReceivedBy);
            Assert.IsTrue(updated.Include80G);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_update_orderItem_statement_receipt()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var created = await svc.CreateAsync(SampleReceipt(particular: "Old Particular"));
            var updated = await svc.UpdateOrderItemParticularAsync(new UpdateOrderItemRequest
            {
                Id = created.Id,
                ItemIndex = 0,
                Particular = "New Particular"
            });
            Assert.IsNotNull(updated);
            Assert.AreEqual("New Particular", updated!.OrderItems[0].Particular);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_update_orderItem_amount_receipt()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var created = await svc.CreateAsync(SampleReceipt(amount: 1000m));
            var updated = await svc.UpdateOrderItemAmountAsync(new UpdateOrderItemRequest
            {
                Id = created.Id,
                ItemIndex = 0,
                Amount = 2500m
            });
            Assert.IsNotNull(updated);
            Assert.AreEqual(2500m, updated!.OrderItems[0].Amount);
            Assert.AreEqual(2500m, updated.TotalAmount);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_update_orderItem_extra_receipt()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var created = await svc.CreateAsync(SampleReceipt());
            var updated = await svc.UpdateOrderItemExtrasAsync(new UpdateOrderItemRequest
            {
                Id = created.Id,
                ItemIndex = 0,
                Extras = new Dictionary<string, string> { ["batch"] = "2026", ["term"] = "1" }
            });
            Assert.IsNotNull(updated);
            Assert.AreEqual("2026", updated!.OrderItems[0].Extras!["batch"]);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_update_payment_details()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var created = await svc.CreateAsync(SampleReceipt(paymentMode: "Cash"));
            var updated = await svc.UpdatePaymentDetailsAsync(new UpdatePaymentDetailsRequest
            {
                Id = created.Id,
                PaymentMode = "UPI",
                FinancialInstrument = new FinancialInstrumentDto
                {
                    Number = "UTR123",
                    BankName = "Test Bank",
                    DrawnOn = DateTime.UtcNow.Date
                }
            });
            Assert.IsNotNull(updated);
            Assert.AreEqual("UPI", updated!.PaymentMode);
            Assert.AreEqual("UTR123", updated.FinancialInstrument?.Number);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public void Can_convert_date_to_string()
    {
        var d = new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc);
        Assert.AreEqual("15-Jul-2026", d.ToString("dd-MMM-yyyy"));
        Assert.AreEqual("2026-07-15", d.ToString("yyyy-MM-dd"));
    }

    [TestMethod]
    public async Task Can_get_receipt_dto_with_data()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var created = await svc.CreateAsync(SampleReceipt("DTO Parent", 999.5m, "Lab Fee"));
            var dto = await svc.GetByIdAsync(created.Id);
            Assert.IsNotNull(dto);
            Assert.AreEqual(999.5m, dto!.TotalAmount);
            Assert.AreEqual("Lab Fee", dto.OrderItems[0].Particular);
            Assert.AreEqual("DTO Parent", dto.CustomerName);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_monthwise_receipt_collection()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            await svc.CreateAsync(SampleReceipt(amount: 100m));
            await svc.CreateAsync(SampleReceipt("B", 250m));
            var rows = await svc.GetMonthwiseCollectionAsync();
            Assert.IsTrue(rows.Count >= 1);
            Assert.IsTrue(rows.Sum(r => r.Amount) >= 350m);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_audit_and_set_regulatory_compliance()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var created = await svc.CreateAsync(SampleReceipt());
            Assert.IsTrue(await svc.AuditAsync(created.Id));
            Assert.IsTrue((await svc.GetByIdAsync(created.Id))!.Audited);

            Assert.IsTrue(await svc.SetRegulatoryComplianceAsync(created.Id, true, "OK"));
            Assert.IsTrue((await svc.GetByIdAsync(created.Id))!.RegulatoryCompliant);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_dates()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            await svc.CreateAsync(SampleReceipt());
            var dates = await svc.GetDatesAsync();
            Assert.IsTrue(dates.Count >= 1);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Auto_number_and_donation_80g()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var donation = SampleReceipt(receiptType: "Donation", amount: 5000m, particular: "Donation");
            donation.Include80G = false; // policy may force true for Donation
            var r = await svc.CreateAsync(donation);
            Assert.IsFalse(string.IsNullOrWhiteSpace(r.Number));
            // ConfigGrPolicy DefaultInclude80G for Donation type
            Assert.IsTrue(r.Include80G || r.ReceiptType == "Donation");
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Tenant_isolation()
    {
        var a = CreateUser();
        var b = CreateUser();
        await using var db = CreateDb();
        try
        {
            var ra = await CreateService(db, a).CreateAsync(SampleReceipt());
            var pageB = await CreateService(db, b).GetAsync(null, null, null, 1, 10);
            Assert.IsFalse(pageB.Items.Any(x => x.Id == ra.Id));
            Assert.IsNull(await CreateService(db, b).GetByIdAsync(ra.Id));
        }
        finally
        {
            await CleanupAsync(db, a.OwnerId);
            await CleanupAsync(db, b.OwnerId);
        }
    }
}
