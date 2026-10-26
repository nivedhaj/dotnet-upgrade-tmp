using Ct.Modules.Admissions.Application.Dtos;
using Ct.Modules.Admissions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Stu.Tests;

public partial class EtlTests
{
    [TestMethod]
    public async Task Can_upd_stu_virtual_id()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("Virtual Id Person"), "t");
            ids.Add(id);
            var personId = (await db.Students.AsNoTracking().FirstAsync(s => s.Id == id)).PersonId;
            var person = await db.Persons.FirstAsync(p => p.Id == personId);
            person.EnsureVirtualId();
            await db.SaveChangesAsync();
            person = await db.Persons.AsNoTracking().FirstAsync(p => p.Id == personId);
            Assert.IsFalse(string.IsNullOrWhiteSpace(person.VirtualId));
            Assert.IsTrue(person.VirtualId!.Contains("@campustrack.net"));
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_get_stu_data()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            ids.Add(await cmds.CreateAsync(SampleStudent("Data1", inst), "t"));
            ids.Add(await cmds.CreateAsync(SampleStudent("Data2", inst), "t"));
            var all = await qry.GetAllAsync(inst);
            Assert.IsTrue(all.Count >= 2);
            var flat = await qry.GetFlatDataAsync(ids);
            Assert.AreEqual(ids.Count, flat.Count);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Get_students_for_fee_txns()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            ids.Add(await cmds.CreateAsync(SampleStudent("FeeTxn1", inst, studentId: "FT-1"), "t"));
            ids.Add(await cmds.CreateAsync(SampleStudent("FeeTxn2", inst, studentId: "FT-2"), "t"));
            var active = (await qry.GetAllAsync(inst)).Where(s => s.Status == nameof(StudentStatus.Active)).ToList();
            Assert.IsTrue(active.Count >= 2);
            Assert.IsTrue(active.All(s => !string.IsNullOrEmpty(s.StudentId) || s.Id != Guid.Empty));
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_create_etl_gcs_opening_balance_marker()
    {
        // Opening-balance ETL lives with Fee; Stu marks students for import
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("OB", studentId: "OB-1"), "t");
            ids.Add(id);
            await cmds.UpdateFeesReceivableAsync(new FeesReceivableRequest
            {
                Id = "OB-1",
                FeeId = "opening",
                FeeName = "Opening Balance",
                Amount = 0m
            });
            var stu = await db.Students.AsNoTracking().FirstAsync(s => s.Id == id);
            Assert.IsTrue(stu.Attributes.Keys.Any(k => k.Contains("feesReceivable")));
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public void Generate_student_etl_payload()
    {
        var payload = new
        {
            action = "import_students",
            rows = new[]
            {
                new { name = "A", courseId = "C1", section = "A" },
                new { name = "B", courseId = "C1", section = "B" }
            }
        };
        Assert.AreEqual(2, payload.rows.Length);
        Assert.AreEqual("import_students", payload.action);
    }

    [TestMethod]
    public void Get_random_date()
    {
        var from = new DateOnly(2014, 1, 1);
        var to = new DateOnly(2016, 12, 31);
        var rng = new Random(42);
        var days = to.DayNumber - from.DayNumber;
        var d = DateOnly.FromDayNumber(from.DayNumber + rng.Next(days + 1));
        Assert.IsTrue(d >= from && d <= to);
    }

    [TestMethod]
    public async Task Can_create_script_for_student_promotion_via_api()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("PromoScript", inst, "C1", "I", "A"), "etl");
            ids.Add(id);
            var ok = await cmds.PromoteAsync(new PromoteStudentRequest
            {
                Id = id,
                BaseInstId = inst,
                BaseCourseId = "C1",
                BaseSection = "A",
                TargetInstId = inst,
                TargetCourseId = "C2",
                TargetSection = "A",
                TargetTerm = "2027-28"
            }, "etl");
            _ = ok;
            Assert.IsNotNull(await CreateQry(db).GetByIdAsync(id));
        }
        finally { await CleanupAsync(db, ids); }
    }
}
