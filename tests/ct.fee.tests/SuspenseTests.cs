using Ct.Fee.Application;
using Ct.Fee.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Fee.Tests;

[TestClass]
public class SuspenseTests : TestBase
{
    [TestMethod]
    public async Task Can_create_and_list_suspense_transaction()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            var s = await svc.CreateSuspenseAsync(new SuspenseTransactionRequest(
                DateTimeOffset.UtcNow, 1500m, "NEFT", "Unknown payer", "UTR-1"));
            Assert.AreEqual(1500m, s.Amount);
            Assert.AreEqual(SuspenseStatus.Unidentified, s.Status);
            Assert.IsTrue((await svc.GetSuspenseTransactionsAsync()).Any(x => x.Id == s.Id));
            Assert.IsNotNull(await svc.GetSuspenseTransactionAsync(s.Id));
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_allocate_suspense_to_fee_transaction()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            var (_, tx) = await SeedPaidFeeAsync(svc, "stu-s1", 3000m, 1000m);
            var s = await svc.CreateSuspenseAsync(new SuspenseTransactionRequest(
                DateTimeOffset.UtcNow, 1000m, "NEFT", "Alloc test", "UTR-2"));
            var allocated = await svc.AllocateSuspenseAsync(s.Id, new SuspenseAllocationRequest(tx.Id, 1000m));
            Assert.AreEqual(SuspenseStatus.Allocated, allocated.Status);
            Assert.AreEqual(1000m, allocated.AllocatedAmount);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_partially_allocate_and_set_status()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            var (_, tx) = await SeedPaidFeeAsync(svc, "stu-s2", 5000m, 500m);
            var s = await svc.CreateSuspenseAsync(new SuspenseTransactionRequest(
                DateTimeOffset.UtcNow, 1000m, "NEFT", "Partial", "UTR-3"));
            var allocated = await svc.AllocateSuspenseAsync(s.Id, new SuspenseAllocationRequest(tx.Id, 400m));
            Assert.AreEqual(SuspenseStatus.PartiallyAllocated, allocated.Status);
            Assert.IsTrue(await svc.SetSuspenseStatusAsync(s.Id, SuspenseStatus.Reversed));
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }
}
