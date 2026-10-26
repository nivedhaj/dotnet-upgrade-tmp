using Ct.Fee.Application;
using Ct.Fee.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Fee.Tests;

[TestClass]
public class RefundTests : TestBase
{
    [TestMethod]
    public async Task Can_create_and_list_refund()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            var (_, tx) = await SeedPaidFeeAsync(svc, "stu-r1", 5000m, 2500m);
            var refund = await svc.CreateRefundAsync(new RefundRequest(
                "stu-r1", tx.Id, DateTimeOffset.UtcNow, 500m, "Overpayment", "RF-001"));
            Assert.AreEqual(500m, refund.Amount);
            Assert.AreEqual(RefundStatus.Pending, refund.Status);
            Assert.IsTrue((await svc.GetRefundsAsync()).Any(r => r.Id == refund.Id));
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_set_refund_status()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            var refund = await svc.CreateRefundAsync(new RefundRequest(
                "stu-r2", null, DateTimeOffset.UtcNow, 100m, "Test", null));
            Assert.IsTrue(await svc.SetRefundStatusAsync(refund.Id, RefundStatus.Approved));
            Assert.IsTrue(await svc.SetRefundStatusAsync(refund.Id, RefundStatus.Paid));
            Assert.AreEqual(RefundStatus.Paid, (await svc.GetRefundsAsync()).First(r => r.Id == refund.Id).Status);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }
}
