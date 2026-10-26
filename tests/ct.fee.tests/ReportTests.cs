using Ct.Fee.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Fee.Tests;

/// <summary>Port of original ReportTests — every report endpoint exercised.</summary>
[TestClass]
public class ReportTests : TestBase
{
    private static ReportFilterRequest F(string? studentId = null) => DefaultFilter(studentId);

    [TestMethod]
    public async Task Can_get_fee_structure()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        var reports = CreateReportService(db, user);
        try
        {
            await fees.CreateFeeAsync(SampleFeeRequest("Structure Fee", 5000m, courseId: "C1"));
            var rows = await reports.GetFeeStructureAsync();
            Assert.IsTrue(rows.Count >= 1);

            var viaFee = await fees.ReportCoursewiseFeeStructureAsync(F());
            Assert.AreEqual("CoursewiseFeeStructure", viaFee.ReportName);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_datewise_fee_collection()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        var reports = CreateReportService(db, user);
        try
        {
            await SeedPaidFeeAsync(fees, "stu-dw");
            var r = await fees.ReportDatewiseFeeCollectionAsync(F("stu-dw"));
            Assert.AreEqual("DatewiseFeeCollection", r.ReportName);
            Assert.IsNotNull(r.Data);

            var detailed = await reports.GetDatewiseFeeCollectionAsync(F("stu-dw"));
            Assert.IsNotNull(detailed);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_datewise_collection_grouped_report()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        try
        {
            await SeedPaidFeeAsync(fees);
            var r = await fees.ReportDatewiseFeeCollectionGroupedAsync(F());
            Assert.IsNotNull(r.Data);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_fee_txn_for_fee_id()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        try
        {
            var (fee, _) = await SeedPaidFeeAsync(fees, "stu-fid");
            var ids = await fees.GetTransactedFeesAsync();
            Assert.IsTrue(ids.Contains(fee.Id));
            var txs = await fees.GetTransactionsAsync("stu-fid");
            Assert.IsTrue(txs.Count >= 1);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_receipt_transaction_report()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        try
        {
            await SeedPaidFeeAsync(fees, "stu-rcp");
            var r = await fees.ReportReceiptTransactionAsync(F("stu-rcp"), asCsv: false);
            Assert.AreEqual("ReceiptTransaction", r.ReportName);
            var csv = await fees.ReportReceiptTransactionAsync(F("stu-rcp"), asCsv: true);
            Assert.AreEqual("ReceiptTransactionCSV", csv.ReportName);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_fee_defaulters_report()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        var reports = CreateReportService(db, user);
        try
        {
            await SeedPaidFeeAsync(fees, "stu-def", 10000m, 1000m); // partial pay → balance
            var r = await fees.ReportFeesDefaultersAsync(F(), version: 1);
            Assert.IsTrue(r.ReportName.Contains("Defaulters"));

            var json = await fees.ReportFeesDefaultersAsync(F(), version: 2);
            Assert.IsTrue(json.ReportName.Contains("v2"));

            var detailed = await reports.GetFeesDefaultersAsync(F());
            Assert.IsNotNull(detailed);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_studentwise_transaction_report()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        try
        {
            await SeedPaidFeeAsync(fees, "stu-swr");
            var r = await fees.ReportStudentwiseTransactionAsync(F("stu-swr"));
            Assert.AreEqual("StudentwiseTransaction", r.ReportName);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_course_studentwise_report()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        try
        {
            await SeedPaidFeeAsync(fees, "stu-csw");
            var r = await fees.ReportCourseStudentwiseTransactionAsync(F("stu-csw"));
            Assert.IsNotNull(r.Data);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_adjustment_transaction_report()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        try
        {
            var fee = await fees.CreateFeeAsync(SampleFeeRequest("Adj Fee", 5000m, "stu-adj"));
            await fees.CreateTransactionAsync(new FeeTransactionRequest(
                "TX-ADJ", DateTimeOffset.UtcNow, "stu-adj", "Cash", false, null, "Cash", null,
                [new TxLine(fee.Id, 1, 1000m)], [], [],
                [new AdjustmentLine("Opening", 200m)]));
            var r = await fees.ReportAdjustmentTransactionsAsync(F());
            Assert.AreEqual("AdjustmentTransactions", r.ReportName);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_fees_paid_report_and_csv()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        try
        {
            await SeedPaidFeeAsync(fees, "stu-paid");
            var r = await fees.ReportFeesPaidAsync(F("stu-paid"), asCsv: false);
            Assert.AreEqual("FeesPaid", r.ReportName);
            var csv = await fees.ReportFeesPaidAsync(F("stu-paid"), asCsv: true);
            Assert.AreEqual("FeesPaidCSV", csv.ReportName);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_adjustment_balance_report()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        try
        {
            await SeedPaidFeeAsync(fees);
            var r = await fees.ReportAdjustmentBalanceAsync(F());
            Assert.IsNotNull(r);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_expected_fees_report()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        try
        {
            await fees.CreateFeeAsync(SampleFeeRequest("ExpR", 3000m));
            var r = await fees.ReportFeesExpectedAsync(F());
            Assert.AreEqual("FeesExpected", r.ReportName);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_fees_reminder()
    {
        // Payable balances act as reminder input
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        try
        {
            await SeedPaidFeeAsync(fees, "stu-rem", 8000m, 1000m);
            var r = await fees.ReportFeesPayableAsync(F("stu-rem"));
            Assert.IsNotNull(r.Data);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_advance_balance_report()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        try
        {
            await SeedPaidFeeAsync(fees);
            var r = await fees.ReportAdvanceBalanceAsync(F());
            Assert.IsNotNull(r);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_coursewise_fee_structure_report()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        var reports = CreateReportService(db, user);
        try
        {
            await fees.CreateFeeAsync(SampleFeeRequest("CW", 2000m, courseId: "X"));
            var r = await fees.ReportCoursewiseFeeStructureAsync(DefaultFilter(courseId: "X"));
            Assert.IsNotNull(r.Data);
            var detailed = await reports.GetCoursewiseFeeStructureAsync(F());
            Assert.IsNotNull(detailed);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_payment_mode_breakup_report()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        try
        {
            await SeedPaidFeeAsync(fees, "stu-pm");
            var r = await fees.ReportPaymentModeBreakupAsync(F());
            Assert.IsNotNull(r);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_print_fee_receipt()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        try
        {
            var (_, tx) = await SeedPaidFeeAsync(fees, "stu-pr");
            var r = await fees.ReportPrintFeeReceiptAsync(new EmailFeeReceiptRequest(tx.Id, "stu-pr", null));
            Assert.IsNotNull(r);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_fees_paid_after_academic_year_report()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        try
        {
            await SeedPaidFeeAsync(fees);
            var r = await fees.ReportFeesPaidAfterAcademicYearAsync();
            Assert.IsNotNull(r);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_fees_paid_by_tc_students_report()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        try
        {
            await SeedPaidFeeAsync(fees);
            var r = await fees.ReportFeesPaidByTcStudentsAsync();
            Assert.IsNotNull(r);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_fees_received_in_other_account_report()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        try
        {
            await SeedPaidFeeAsync(fees);
            var r = await fees.ReportFeeReceivedInOtherAccountAsync(F());
            Assert.IsNotNull(r);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_refund_report()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        try
        {
            await fees.CreateRefundAsync(new RefundRequest("stu-rr", null, DateTimeOffset.UtcNow, 50m, "r", null));
            var r = await fees.ReportRefundAsync(F());
            Assert.AreEqual("Refund", r.ReportName);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_concession_report()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        try
        {
            var fee = await fees.CreateFeeAsync(SampleFeeRequest("Conc", 5000m, "stu-con"));
            await fees.CreateTransactionAsync(new FeeTransactionRequest(
                "TX-CON", DateTimeOffset.UtcNow, "stu-con", "Cash", false, null, "Cash", null,
                [new TxLine(fee.Id, 1, 2000m)], [], [new DiscountLine("Concession", 100m)], []));
            var r = await fees.ReportConcessionAsync(F());
            Assert.IsNotNull(r);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_suspense_account_txn_report()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        try
        {
            await fees.CreateSuspenseAsync(new SuspenseTransactionRequest(
                DateTimeOffset.UtcNow, 100m, "NEFT", "x", "u"));
            var r = await fees.ReportSuspenseAccountTxnAsync(F());
            Assert.AreEqual("SuspenseAccountTxn", r.ReportName);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_fee_via_report_path()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        try
        {
            var created = await fees.CreateFeeAsync(SampleFeeRequest("Rpt Fee"));
            var fee = await fees.GetFeeAsync(created.Id);
            Assert.IsNotNull(fee);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_touch_tx()
    {
        // Port of can_touch_tx — update modified stamp via status set
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        try
        {
            var (_, tx) = await SeedPaidFeeAsync(fees, "stu-touch");
            Assert.IsTrue(await fees.SetTransactionStatusAsync(tx.Id, TransactionStatus.Completed));
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_breakups_fine_late_timeline()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        try
        {
            var fee = await fees.CreateFeeAsync(SampleFeeRequest("FineR", 2000m, "stu-fn"));
            await fees.CreateTransactionAsync(new FeeTransactionRequest(
                "TX-FN", DateTimeOffset.UtcNow, "stu-fn", "Cash", false, null, "Cash", null,
                [new TxLine(fee.Id, 1, 500m)], [new TxLine(fee.Id, 1, 50m)], [], []));

            Assert.IsNotNull(await fees.ReportBreakUpsAsync(F()));
            Assert.IsNotNull(await fees.ReportFineCollectedAsync(F()));
            Assert.IsNotNull(await fees.ReportLatePaymentAsync(F()));
            Assert.IsNotNull(await fees.ReportTimeLineAsync(F()));
            Assert.IsNotNull(await fees.ReportTxHistoryAsync());
            Assert.IsNotNull(await fees.ReportCoursewiseFeeBreakupAsJsonAsync(F()));
            Assert.IsNotNull(await fees.ReportCoursewiseFeeCollectionAsJsonAsync(F()));
            Assert.IsNotNull(await fees.ReportCoursewiseFeeStructureAsJsonAsync());
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Collection_accounts_from_policy()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var fees = CreateFeeService(db, user);
        var accounts = await fees.GetFeeCollectionAccountsAsync();
        Assert.IsTrue(accounts.Count > 0);
    }
}
