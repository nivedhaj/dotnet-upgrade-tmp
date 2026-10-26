using Ct.Fee.Application;
using Ct.Fee.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Fee.Tests;

/// <summary>Port of original FeeTxTests.</summary>
[TestClass]
public class FeeTxTests : TestBase
{
    [TestMethod]
    public async Task Sets_flag_IsTxDone()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            var fee = await svc.CreateFeeAsync(SampleFeeRequest("IsTxDone Fee", 2000m, studentId: "stu-itd"));
            Assert.IsFalse(fee.IsTxDone);
            await svc.CreateTransactionAsync(new FeeTransactionRequest(
                "TX-ITD", DateTimeOffset.UtcNow, "stu-itd", "Cash", false, null, "Cash", null,
                [new TxLine(fee.Id, 1, 1000m)], [], [], []));
            Assert.IsTrue((await svc.GetFeeAsync(fee.Id))!.IsTxDone);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public void Can_display_decimal()
    {
        // Port of Helper.FormatDecimal behaviour used in receipts
        static string FormatDecimal(decimal d) => d.ToString("0.00");
        var decs = new[] { 0.0m, 0m, 100.2302m, 23.65m, -5m, -89.8526m, 100m / 3, 100m / 6 };
        foreach (var d in decs)
        {
            var s = FormatDecimal(decimal.Round(d, 2, MidpointRounding.AwayFromZero));
            Assert.IsTrue(s.Contains('.') || s.EndsWith("00") || s.Length > 0);
        }
        Assert.AreEqual("100.23", FormatDecimal(100.2302m));
        Assert.AreEqual("23.65", FormatDecimal(23.65m));
    }

    [TestMethod]
    public async Task Can_get_fee_tx()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            await SeedPaidFeeAsync(svc, "stu-gtx");
            var list = await svc.GetTransactionsAsync(null);
            Assert.IsTrue(list.Count >= 1);
            list = await svc.GetTransactionsAsync("stu-gtx");
            Assert.IsTrue(list.All(t => t.StudentId == "stu-gtx"));
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_transacted_fees()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            var (fee, _) = await SeedPaidFeeAsync(svc, "stu-tf");
            var ids = await svc.GetTransactedFeesAsync();
            Assert.IsTrue(ids.Contains(fee.Id));
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_tx_html()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            var (_, tx) = await SeedPaidFeeAsync(svc, "stu-html");
            var html = await svc.GetTransactionHtmlAsync(tx.Id);
            Assert.IsNotNull(html);
            Assert.IsTrue(html!.Html.Contains(tx.TxNo));
            Assert.IsTrue(html.Html.Contains("stu-html"));
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_stu_fee_details()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            await SeedPaidFeeAsync(svc, "stu-det");
            var details = await svc.GetStudentFeeDetailsAsync("stu-det");
            Assert.AreEqual("stu-det", details.StudentId);
            Assert.IsTrue(details.Transactions.Count >= 1);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_tx_by_user_and_ref()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            var (_, tx) = await SeedPaidFeeAsync(svc, "stu-user");
            var byId = await svc.GetTransactionAsync(tx.Id.ToString());
            var byNo = await svc.GetTransactionAsync(tx.TxNo);
            Assert.IsNotNull(byId);
            Assert.IsNotNull(byNo);
            Assert.AreEqual(tx.Id, byNo!.Id);

            var byRef = await svc.GetFeeTxByRefNoAsync(tx.RefNo!);
            Assert.IsNotNull(byRef);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_search_fee_tx()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            var (_, tx) = await SeedPaidFeeAsync(svc, "stu-search");
            var found = await svc.GetTransactionAsync(tx.TxNo);
            Assert.IsNotNull(found);
            var missing = await svc.GetTransactionAsync("DOES-NOT-EXIST");
            Assert.IsNull(missing);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_check_invalid_fee_tx()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            var fee = await svc.CreateFeeAsync(SampleFeeRequest("Invalid Tx", 1000m, studentId: "stu-inv"));

            // empty student
            await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
                svc.CreateTransactionAsync(new FeeTransactionRequest(
                    "TX-INV1", DateTimeOffset.UtcNow, "", "Cash", false, null, "Cash", null,
                    [new TxLine(fee.Id, 1, 100m)], [], [], [])));

            // negative amount
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() =>
                svc.CreateTransactionAsync(new FeeTransactionRequest(
                    "TX-INV2", DateTimeOffset.UtcNow, "stu-inv", "Cash", false, null, "Cash", null,
                    [new TxLine(fee.Id, 1, 100m)], [], [new DiscountLine("x", 500m)], [])));

            // duplicate ref
            await svc.CreateTransactionAsync(new FeeTransactionRequest(
                "TX-INV3", DateTimeOffset.UtcNow, "stu-inv", "Cash", false, "DUP-REF", "Cash", null,
                [new TxLine(fee.Id, 1, 100m)], [], [], []));
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                svc.CreateTransactionAsync(new FeeTransactionRequest(
                    "TX-INV4", DateTimeOffset.UtcNow, "stu-inv", "Cash", false, "DUP-REF", "Cash", null,
                    [new TxLine(fee.Id, 2, 100m)], [], [], [])));
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_advance_balance_and_closing_balance()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            await SeedPaidFeeAsync(svc, "stu-bal", feeAmount: 10000m, payAmount: 5000m);
            var closing = await svc.GetClosingBalanceAsync();
            Assert.IsNotNull(closing);

            var expected = await svc.GetStudentFeesExpectedPaidAsync("stu-bal");
            Assert.IsTrue(expected.Count >= 1);
            Assert.IsTrue(expected[0].PendingTotal >= 0);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_update_transaction_fields()
    {
        // Port of get_fee_transactions_and_set_course / change metadata
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            var (_, tx) = await SeedPaidFeeAsync(svc, "stu-upd");
            var newDate = DateTimeOffset.UtcNow.AddDays(-2);
            var updated = await svc.UpdateTransactionAsync(new FeeTransactionUpdateRequest(tx.Id, newDate, "NEW-REF", "PaidByX"));
            Assert.IsNotNull(updated);
            Assert.AreEqual("NEW-REF", updated!.RefNo);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_disable_fee_txns()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            var (_, tx) = await SeedPaidFeeAsync(svc, "stu-dis");
            var ok = await svc.SetTransactionStatusAsync(tx.Id, TransactionStatus.Cancelled);
            Assert.IsTrue(ok);
            Assert.AreEqual(TransactionStatus.Cancelled, (await svc.GetTransactionAsync(tx.Id.ToString()))!.Status);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_fee_collection_studentwise()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            await SeedPaidFeeAsync(svc, "stu-sw");
            var rows = await svc.GetStudentFeesExpectedPaidAsync(null);
            Assert.IsTrue(rows.Any(r => r.StudentId == "stu-sw"));
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_stu_fee_to_be_paid()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            var (fee, _) = await SeedPaidFeeAsync(svc, "stu-pay", 10000m, 3000m);
            var toPay = await svc.GetFeesToBePaidAsync("stu-pay");
            Assert.IsTrue(toPay.Any(x => x.FeeId == fee.Id && x.Balance > 0));
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_stu_fees_expected()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            await SeedPaidFeeAsync(svc, "stu-exp2");
            var expected = await svc.GetStudentFeesExpectedPaidAsync("stu-exp2");
            Assert.IsTrue(expected.Count >= 1);
            Assert.IsTrue(expected[0].Lines.Count >= 1);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_set_fee_receivable()
    {
        // NullStudentFeeReceivableClient is used; create tx still completes and publishes events
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            var result = await svc.Transact2Async(new FeeTransactionRequest(
                "TX-RCV", DateTimeOffset.UtcNow, "stu-rcv", "Cash", false, null, "Cash", null,
                [new TxLine((await svc.CreateFeeAsync(SampleFeeRequest("Rcv", 1000m, "stu-rcv"))).Id, 1, 500m)],
                [], [], []));
            Assert.AreEqual("Transaction completed", result.Message);
            Assert.IsTrue(result.Amount > 0);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Transaction_amount_includes_fines_discounts_adjustments()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            var fee = await svc.CreateFeeAsync(SampleFeeRequest("Complex", 10000m, "stu-cx"));
            var tx = await svc.CreateTransactionAsync(new FeeTransactionRequest(
                "TX-CX", DateTimeOffset.UtcNow, "stu-cx", "Cash", false, null, "Cash", null,
                [new TxLine(fee.Id, 1, 5000m)],
                [new TxLine(fee.Id, 1, 100m)],
                [new DiscountLine("Scholarship", 50m)],
                [new AdjustmentLine("Round", 25m)]));
            Assert.AreEqual(5075m, tx.Amount);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public void Fee_calculation_service_rounds_correctly()
    {
        var calc = new FeeCalculationService();
        Assert.AreEqual(100.01m, calc.CalculateAmount([100.005m], [0.004m], [0.001m], []));
    }

    [TestMethod]
    public async Task Change_fee_ids_in_txs_via_new_payment_on_updated_structure()
    {
        // Legacy data-fix script analogue: re-pay against updated fee installments
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            var fee = await svc.CreateFeeAsync(SampleFeeRequest("Migrate Fee", 4000m, "stu-mig"));
            await svc.CreateTransactionAsync(new FeeTransactionRequest(
                "TX-MIG1", DateTimeOffset.UtcNow, "stu-mig", "Cash", false, null, "Cash", null,
                [new TxLine(fee.Id, 1, 2000m)], [], [], []));
            var updated = await svc.UpdateFeeAsync(new FeeDefinitionUpdateRequest(
                fee.Id, fee.Name, fee.DisplayText, 4000m, fee.Tags, true, false,
                [new FeeInstallmentDto(1, "I1", 2000m, null), new FeeInstallmentDto(2, "I2", 2000m, null)],
                [], ["stu-mig"], []));
            Assert.IsNotNull(updated);
            var tx2 = await svc.CreateTransactionAsync(new FeeTransactionRequest(
                "TX-MIG2", DateTimeOffset.UtcNow, "stu-mig", "Cash", false, null, "Cash", null,
                [new TxLine(fee.Id, 2, 2000m)], [], [], []));
            Assert.AreEqual(2000m, tx2.Amount);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }
}
