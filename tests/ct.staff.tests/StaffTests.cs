using Ct.Staff.Application;
using Ct.Staff.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Staff.Tests;

/// <summary>Port of original StaffTests.</summary>
[TestClass]
public class StaffTests : TestBase
{
    [TestMethod]
    public async Task Can_create_staff()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var created = await svc.CreateAsync(SampleStaff("Alice Teacher"));
            var id = IdOf(created);
            Assert.AreNotEqual(Guid.Empty, id);
            var loaded = await svc.GetByIdAsync(id);
            Assert.IsNotNull(loaded);
            Assert.AreEqual("Alice Teacher", PropString(loaded!, "Name"));
            Assert.AreEqual(nameof(StaffStatus.Active), PropString(loaded!, "Status"));
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_qry_staff()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            await svc.CreateAsync(SampleStaff("Bob", email: "bob@ex.com"));
            await svc.CreateAsync(SampleStaff("Cara", email: "cara@ex.com"));
            var all = await svc.GetAllAsync();
            Assert.IsTrue(all.Count >= 2);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_qry_staff_for_facet()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            await svc.CreateAsync(SampleStaff("T1", staffType: "Teaching", instituteIds: ["I1"]));
            await svc.CreateAsync(SampleStaff("N1", email: "n1@ex.com", staffType: "NonTeaching", designation: "Clerk", instituteIds: ["I1"]));
            var facet = await svc.FacetAsync();
            Assert.IsNotNull(facet);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_mobiles()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            await svc.CreateAsync(SampleStaff("Mob", mobile: "9000000001", email: "m@ex.com"));
            var all = await svc.GetAllAsync();
            Assert.IsTrue(all.Any(o => PropString(o, "PrimaryMobile") == "9000000001"));
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_send_sms_recipients()
    {
        // Port of can_send_sms — resolve recipients only
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            await svc.CreateAsync(SampleStaff("Teach", staffType: "Teaching", mobile: "9111111111", email: "t@ex.com"));
            var recipients = await svc.GetSmsRecipientsAsync(["*"], null, null);
            Assert.IsNotNull(recipients);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_add_new_employment_to_staff()
    {
        var user = CreateUser(instId: "I-HOME");
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var id = IdOf(await svc.CreateAsync(SampleStaff("Emp", email: "emp@ex.com", instituteIds: ["I-HOME"])));
            Assert.IsTrue(await svc.EmployAsync(id, "I-NEW", "Senior Teacher", "REF-1"));
            var loaded = await db.Staff.AsNoTracking().FirstAsync(s => s.Id == id);
            Assert.IsTrue(loaded.EmploymentHistory.Any(e => e.InstId == "I-NEW"));
            Assert.IsTrue(loaded.InstituteIds.Contains("I-NEW"));
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_all_teaching_staff_for_institute()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            await svc.CreateAsync(SampleStaff("T-Inst", staffType: "Teaching", instituteIds: ["INST-A"], email: "ti@ex.com"));
            await svc.CreateAsync(SampleStaff("Clerk", staffType: "NonTeaching", instituteIds: ["INST-A"], email: "cl@ex.com", designation: "Clerk"));
            var forInst = await svc.ForInstituteAsync("INST-A");
            Assert.IsTrue(forInst.Count >= 2);
            var teaching = forInst.Where(o => PropString(o, "StaffType") == "Teaching").ToList();
            Assert.IsTrue(teaching.Count >= 1);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_staff_count_institutewise_and_genderwise()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            await svc.CreateAsync(SampleStaff("F1", gender: "Female", instituteIds: ["G1"], email: "f1@ex.com"));
            var req = SampleStaff("M1", instituteIds: ["G1"], email: "m1@ex.com");
            req.Gender = "Male";
            await svc.CreateAsync(req);
            var byInst = await svc.GetStaffCountInstitutewiseAsync();
            var byGender = await svc.GetStaffCountInstituteGenderwiseAsync();
            Assert.IsNotNull(byInst);
            Assert.IsNotNull(byGender);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_enable_disable_update_and_contacts()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var id = IdOf(await svc.CreateAsync(SampleStaff("Upd", email: "upd@ex.com")));
            Assert.IsTrue(await svc.SetPrimaryEmailAsync(id, "new@ex.com"));
            Assert.IsTrue(await svc.SetPrimaryMobileAsync(id, "9222222222"));
            Assert.IsTrue(await svc.UpdateGenderAsync(id, "Male"));
            Assert.IsTrue(await svc.SetAliasAsync(id, "UP"));
            Assert.IsTrue(await svc.UpdateDesignationAsync(id, "HOD", "I1"));
            Assert.IsTrue(await svc.AssignSubjectsAsync(id, ["Physics", "Chem"]));
            Assert.IsTrue(await svc.AssignInstitutesAsync(id, ["I1", "I2"]));
            Assert.IsTrue(await svc.SetClassTeacherAsync(id, "C1", "A"));
            Assert.IsTrue(await svc.DisableAsync(id));
            // disabled hidden from GetAll
            Assert.IsFalse((await svc.GetAllAsync()).Any(o => IdOf(o) == id));
            Assert.IsTrue(await svc.EnableAsync(id));
            Assert.IsNotNull(await svc.GetByIdAsync(id));
            Assert.IsNotNull(await svc.GetByEmailAsync("new@ex.com"));
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_set_timetable_and_salary()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var id = IdOf(await svc.CreateAsync(SampleStaff("TT", email: "tt@ex.com", instituteIds: ["I1"])));
            Assert.IsTrue(await svc.SetTimetableAsync(id,
            [
                new TtSlot { DayOfWeek = "Monday", Period = 1, Subject = "Math", CourseId = "C1", Section = "A" }
            ]));
            var tt = await svc.GetTimetableAsync(id);
            Assert.IsNotNull(tt);

            Assert.IsTrue(await svc.SalaryAddAsync(id, "I1",
                [new SalaryItem { Name = "Basic", Amount = 30000 }],
                [new SalaryItem { Name = "PF", Amount = 1800 }],
                DateTime.UtcNow, payslip: true));
            var payslip = await svc.GetPayslipAsync(id, null);
            Assert.IsNotNull(payslip);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Set_user_id_for_staff_via_ref()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var id = IdOf(await svc.CreateAsync(SampleStaff("UserLink", email: "ul@ex.com")));
            Assert.IsTrue(await svc.AssignStudentIdAsync(id, "user-ref-9"));
            var entity = await db.Staff.AsNoTracking().FirstAsync(s => s.Id == id);
            Assert.AreEqual("user-ref-9", entity.RefId);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Get_absentees_via_attendance()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            var id = IdOf(await svc.CreateAsync(SampleStaff("Abs", email: "abs@ex.com")));
            Assert.IsTrue(await svc.RecordAttendanceAsync(id, DateTime.UtcNow.Date, "Absent", 1));
            var att = await svc.GetAttendanceAsync(id, DateTime.UtcNow.Date.AddDays(-1), DateTime.UtcNow.Date.AddDays(1));
            Assert.IsNotNull(att);
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
            var id = IdOf(await CreateService(db, a).CreateAsync(SampleStaff("OnlyA", email: "a@ex.com")));
            Assert.IsNull(await CreateService(db, b).GetByIdAsync(id));
        }
        finally
        {
            await CleanupAsync(db, a.OwnerId);
            await CleanupAsync(db, b.OwnerId);
        }
    }
}
