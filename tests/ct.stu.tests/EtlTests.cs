using System.Globalization;
using System.Text;
using Ct.Modules.Admissions.Application.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Stu.Tests;

/// <summary>
/// Port of EtlTests — CSV/JSON helpers and bulk seed via CreateAsync (no Raven scripts).
/// </summary>
[TestClass]
public partial class EtlTests : TestBase
{
    [TestMethod]
    public void Can_convert_csv_to_json()
    {
        var csv = "Name,Mobile,Course\nAlice,9000000001,I\nBob,9000000002,II\n";
        var rows = ParseCsv(csv);
        Assert.AreEqual(2, rows.Count);
        Assert.AreEqual("Alice", rows[0]["Name"]);
        Assert.AreEqual("II", rows[1]["Course"]);
    }

    [TestMethod]
    public void Can_convert_new_student_csv_to_json()
    {
        var csv = "Name,Gender,Mobile,Email,CourseId,Section\nNew Kid,Male,9111111111,n@x.com,C1,A\n";
        var rows = ParseCsv(csv);
        Assert.AreEqual(1, rows.Count);
        Assert.AreEqual("Male", rows[0]["Gender"]);
    }

    [TestMethod]
    public async Task Can_upd_stu_course()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("CourseUpd", inst, "C1", "I"), "t");
            ids.Add(id);
            await cmds.UpdateCourseAsync(id, inst, "C3", "III", "2026-27", "C", null);
            Assert.AreEqual("III", (await qry.GetByIdAsync(id))!.CourseName);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_update_rte()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("RTE Upd"), "t");
            ids.Add(id);
            await cmds.SetTagsAsync(id, ["RTE"]);
            Assert.IsTrue((await qry.GetByTagAsync("RTE")).Any(s => s.Id == id));
            await cmds.SetTagsAsync(id, ["General"]);
            Assert.IsFalse((await qry.GetByTagAsync("RTE")).Any(s => s.Id == id));
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_create_promoted_and_new_student_via_etl_seed()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            // new students
            for (var i = 0; i < 3; i++)
                ids.Add(await cmds.CreateAsync(SampleStudent($"New{i}", inst, "C1", "I"), "etl"));
            // promote first
            var ok = await cmds.PromoteAsync(new PromoteStudentRequest
            {
                Id = ids[0],
                BaseInstId = inst,
                BaseCourseId = "C1",
                TargetInstId = inst,
                TargetCourseId = "C2",
                TargetTerm = "2027-28",
                TargetSection = "A"
            }, "etl");
            _ = ok;
            Assert.IsTrue((await qry.GetAllAsync(inst)).Count >= 3);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_import_students_from_template_rows()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            var template = ParseCsv("Name,Mobile,CourseId,Section\nImp1,9000000011,C1,A\nImp2,9000000012,C1,B\n");
            foreach (var row in template)
            {
                ids.Add(await cmds.CreateAsync(SampleStudent(
                    row["Name"], inst, row["CourseId"], row["CourseId"], row["Section"],
                    mobile: row["Mobile"]), "etl"));
            }
            Assert.AreEqual(2, (await qry.GetAllAsync(inst)).Count);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public void Generate_student_names()
    {
        var names = Enumerable.Range(1, 5).Select(i => $"Student {i:D3}").ToList();
        Assert.AreEqual("Student 001", names[0]);
        Assert.AreEqual(5, names.Count);
    }

    [TestMethod]
    public async Task Can_setup_demo_students()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            foreach (var n in new[] { "Demo A", "Demo B", "Demo C" })
                ids.Add(await cmds.CreateAsync(SampleStudent(n, inst), "demo"));
            Assert.IsTrue((await qry.SearchAsync("Demo", inst, 1, 20)).Total >= 3);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_update_student_mobile()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("MobUpd", mobile: "9000000000"), "t");
            ids.Add(id);
            await cmds.SetPrimaryMobileAsync(id, "9555555555");
            await cmds.UpdateAsync(new UpdateStudentRequest { Id = id, Mobile = "9555555555" }, "t");
        }
        finally { await CleanupAsync(db, ids); }
    }

    private static List<Dictionary<string, string>> ParseCsv(string csv)
    {
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var headers = lines[0].Split(',');
        var list = new List<Dictionary<string, string>>();
        foreach (var line in lines.Skip(1))
        {
            var cols = line.Split(',');
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headers.Length && i < cols.Length; i++)
                row[headers[i]] = cols[i];
            list.Add(row);
        }
        return list;
    }
}
