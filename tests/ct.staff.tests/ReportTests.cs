using Ct.Staff.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Staff.Tests;

[TestClass]
public class ReportTests : TestBase
{
    [TestMethod]
    public async Task Can_get_payslip()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        var reports = CreateReports(svc);
        try
        {
            var id = IdOf(await svc.CreateAsync(SampleStaff("Pay", email: "pay@ex.com", instituteIds: ["I1"])));
            await svc.SalaryAddAsync(id, "I1",
                [new SalaryItem { Name = "Basic", Amount = 40000 }],
                [new SalaryItem { Name = "Tax", Amount = 2000 }],
                DateTime.UtcNow, true);
            var slip = await reports.PayslipAsync(id, null);
            Assert.IsNotNull(slip);

            var list = await reports.StaffListAsync();
            Assert.IsTrue(list.Count >= 1);

            var tt = await reports.TodayTimetableAsync(id);
            Assert.IsNotNull(tt);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_manage_salary_structure()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var salary = CreateSalaryService(db, user);
        try
        {
            var created = await salary.CreateStructureAsync(new CreateSalaryStructureRequest
            {
                Earnings = new Dictionary<string, decimal> { ["Basic"] = 25000, ["HRA"] = 5000 },
                Deductions = new Dictionary<string, decimal> { ["PF"] = 1500 },
                EffectiveFrom = DateTime.UtcNow
            });
            Assert.AreNotEqual(Guid.Empty, IdOf(created));
            var current = await salary.GetCurrentAsync();
            Assert.IsNotNull(current);
            var all = await salary.GetAllAsync();
            Assert.IsTrue(all.Count >= 1);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }
}
