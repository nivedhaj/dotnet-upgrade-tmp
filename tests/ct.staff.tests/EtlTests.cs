using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Staff.Tests;

[TestClass]
public class EtlTests : TestBase
{
    [TestMethod]
    public async Task Can_employ_staff_to_new_institute_based_on_current_inst()
    {
        var user = CreateUser(instId: "HOME");
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var id = IdOf(await svc.CreateAsync(SampleStaff("Move", email: "move@ex.com", instituteIds: ["HOME"])));
            Assert.IsTrue(await svc.EmployAsync(id, "TARGET", "Teacher", null));
            var entity = (await db.Staff.FindAsync(id))!;
            Assert.IsTrue(entity.InstituteIds.Contains("TARGET"));
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_setup_demo_staff()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            foreach (var n in new[] { "Demo1", "Demo2", "Demo3" })
                await svc.CreateAsync(SampleStaff(n, email: $"{n.ToLower()}@demo.com", instituteIds: ["DEMO"]));
            Assert.IsTrue((await svc.ForInstituteAsync("DEMO")).Count >= 3);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Import_sphs_staff()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var rows = new[]
            {
                ("Anita", "anita@sphs.edu", "Teaching", "Teacher"),
                ("Ravi", "ravi@sphs.edu", "NonTeaching", "Admin")
            };
            foreach (var (name, email, type, desig) in rows)
                await svc.CreateAsync(SampleStaff(name, email: email, staffType: type, designation: desig, instituteIds: ["SPHS"]));
            Assert.IsTrue((await svc.ForInstituteAsync("SPHS")).Count >= 2);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_upd_staff_code_ref()
    {
        // Port of can_upd_stu_virtual_id analogue for staff
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var id = IdOf(await svc.CreateAsync(SampleStaff("Vid", email: "vid@ex.com", staffCode: "TMP")));
            await svc.UpdateAsync(id, SampleStaff("Vid", email: "vid@ex.com", staffCode: "STF-001"));
            Assert.AreEqual("STF-001", PropString((await svc.GetByIdAsync(id))!, "StaffCode"));
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }
}
