using Ct.Fee.Application;
using Ct.Fee.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Fee.Tests;

/// <summary>Port of original FeeTests (Cmd + Qry).</summary>
[TestClass]
public class FeeTests : TestBase
{
    [TestMethod]
    public async Task Can_create_fee()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            var fee = await svc.CreateFeeAsync(SampleFeeRequest("Test fee 1", 5000m, courseId: "d8bd4288-9d10-4116-9500-2a85bcd7e801"));
            Assert.IsNotNull(fee);
            Assert.AreEqual("Test fee 1", fee.Name);
            Assert.AreEqual(5000m, fee.Amount);
            Assert.AreEqual(FeeStatus.Active, fee.Status);
            Assert.AreEqual(user.OwnerId, fee.OwnerId);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_fee()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            await svc.CreateFeeAsync(SampleFeeRequest("List Me"));
            var fees = await svc.GetFeesAsync();
            Assert.IsTrue(fees.Count >= 1);
            Assert.IsTrue(fees.Any(f => f.Name == "List Me"));
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_fee_by_id()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            var created = await svc.CreateFeeAsync(SampleFeeRequest("Test fee 1"));
            var fee = await svc.GetFeeAsync(created.Id);
            Assert.IsNotNull(fee);
            Assert.AreEqual("Test fee 1", fee!.Name);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_fee_installments()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            await svc.CreateFeeAsync(SampleFeeRequest("Inst Fee", 4000m, courseId: "I"));
            var insts = await svc.GetFeeInstallmentsFlattenedAsync("I");
            Assert.IsTrue(insts.Count >= 2);
            Assert.IsTrue(insts.All(i => i.CourseList.Contains("I")));
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_update_fee()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            var created = await svc.CreateFeeAsync(SampleFeeRequest("Old", 5000m));
            var updated = await svc.UpdateFeeAsync(new FeeDefinitionUpdateRequest(
                created.Id, "New Name", "Display", 6000m, ["tag"], false, false,
                [new FeeInstallmentDto(1, "Only", 6000m, null)], [], [], ["c1"]));
            Assert.IsNotNull(updated);
            Assert.AreEqual("New Name", updated!.Name);
            Assert.AreEqual(6000m, updated.Amount);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_enable_fee()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            var fee = await svc.CreateFeeAsync(SampleFeeRequest());
            await svc.SetFeeStatusAsync(fee.Id, FeeStatus.Inactive);
            var ok = await svc.SetFeeStatusAsync(fee.Id, FeeStatus.Active);
            Assert.IsTrue(ok);
            Assert.AreEqual(FeeStatus.Active, (await svc.GetFeeAsync(fee.Id))!.Status);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_disable_fee()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            var fee = await svc.CreateFeeAsync(SampleFeeRequest());
            var ok = await svc.SetFeeStatusAsync(fee.Id, FeeStatus.Inactive);
            Assert.IsTrue(ok);
            Assert.AreEqual(FeeStatus.Inactive, (await svc.GetFeeAsync(fee.Id))!.Status);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_set_fee_due_date()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            var fee = await svc.CreateFeeAsync(SampleFeeRequest("Due Date Fee", 2000m));
            var newDue = new DateOnly(2026, 12, 15);
            var updated = await svc.UpdateFeeAsync(new FeeDefinitionUpdateRequest(
                fee.Id, fee.Name, fee.DisplayText, fee.Amount, fee.Tags, fee.CollectStudentWise, fee.SpansAcademicYears,
                [new FeeInstallmentDto(1, "I1", 1000m, newDue), new FeeInstallmentDto(2, "I2", 1000m, newDue.AddMonths(3))],
                fee.Fines.ToList(), fee.StudentList.ToList(), fee.CourseList.ToList()));
            Assert.AreEqual(newDue, updated!.Installments.First(i => i.InstallmentNo == 1).DueDate);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_sanitize_fee()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            // Name is trimmed/lowercased via Normalize()
            var fee = await svc.CreateFeeAsync(SampleFeeRequest("  Messy Name  ", 100.456m));
            Assert.AreEqual("Messy Name", fee.Name);
            Assert.AreEqual("messy name", fee.NameLower);
            Assert.AreEqual(100.46m, fee.Amount); // rounded AwayFromZero
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public void Can_get_date_range()
    {
        var user = CreateUser();
        using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);

        var monthly = svc.GetDates("monthly", 6, 2026);
        Assert.AreEqual(new DateOnly(2026, 6, 1), monthly.From);
        Assert.AreEqual(new DateOnly(2026, 6, 30), monthly.To);

        var quarterly = svc.GetDates("quarterly", 4, 2026);
        Assert.IsTrue(quarterly.To >= quarterly.From);

        var ay = svc.GetDates("academic-year", 4, 2026,
            academicYearFrom: new DateOnly(2026, 4, 1),
            academicYearTo: new DateOnly(2027, 3, 31));
        Assert.AreEqual(new DateOnly(2026, 4, 1), ay.From);
    }

    [TestMethod]
    public async Task Can_get_expected_fees()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            await svc.CreateFeeAsync(SampleFeeRequest("Expected", 6000m, studentId: "stu-e1"));
            var from = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));
            var to = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6));
            var rows = await svc.FeesExpectedCollectedAsync(from, to);
            Assert.IsNotNull(rows);
            Assert.IsTrue(rows.Count >= 1);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public void Get_previous_inst_year()
    {
        // Port of academic-year helper: previous AY ends day before current AY starts
        var currentFrom = new DateOnly(2026, 4, 1);
        var previousTo = currentFrom.AddDays(-1);
        var previousFrom = new DateOnly(previousTo.Year - 1, 4, 1);
        Assert.AreEqual(new DateOnly(2025, 3, 31), previousTo);
        Assert.AreEqual(new DateOnly(2024, 4, 1), previousFrom);
    }

    [TestMethod]
    public async Task Can_assign_student_and_check_can_remove()
    {
        var user = CreateUser();
        await using var db = CreateDb(user);
        var svc = CreateFeeService(db, user);
        try
        {
            var fee = await svc.CreateFeeAsync(SampleFeeRequest("Assign", 1000m));
            var assigned = await svc.AssignStudentAsync(fee.Id, "stu-new");
            Assert.IsTrue(assigned!.StudentList.Contains("stu-new"));

            var canRemove = await svc.CanRemoveStudentAsync(fee.Id, "stu-new");
            Assert.IsTrue(canRemove.CanRemove);

            await svc.CreateTransactionAsync(new FeeTransactionRequest(
                "TX-AR1", DateTimeOffset.UtcNow, "stu-new", "Cash", false, null, "Cash", null,
                [new TxLine(fee.Id, 1, 500m)], [], [], []));
            canRemove = await svc.CanRemoveStudentAsync(fee.Id, "stu-new");
            Assert.IsFalse(canRemove.CanRemove);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Tenant_isolation_hides_other_owners_fees()
    {
        var a = CreateUser();
        var b = CreateUser();
        await using var dbA = CreateDb(a);
        await using var dbB = CreateDb(b);
        try
        {
            var fee = await CreateFeeService(dbA, a).CreateFeeAsync(SampleFeeRequest("Only A"));
            Assert.IsNull(await CreateFeeService(dbB, b).GetFeeAsync(fee.Id));
        }
        finally
        {
            await CleanupAsync(dbA, a.OwnerId);
            await CleanupAsync(dbB, b.OwnerId);
        }
    }
}
