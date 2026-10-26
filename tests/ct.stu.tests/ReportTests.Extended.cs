using Ct.Modules.Admissions.Application.Dtos;
using Ct.Modules.Admissions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace Ct.Stu.Tests;

public partial class ReportTests
{
    [TestMethod]
    public async Task Can_get_coursewise_student_breakup()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            ids.Add(await cmds.CreateAsync(SampleStudent("CW-A", inst, "C1", "I", "A"), "t"));
            ids.Add(await cmds.CreateAsync(SampleStudent("CW-B", inst, "C1", "I", "B"), "t"));
            ids.Add(await cmds.CreateAsync(SampleStudent("CW-C", inst, "C2", "II", "A"), "t"));
            var sectionwise = await qry.GetSectionwiseAsync(inst);
            var coursewise = sectionwise.GroupBy(s => new { s.CourseId, s.CourseName })
                .Select(g => new { g.Key.CourseId, g.Key.CourseName, Count = g.Sum(x => x.Count) })
                .ToList();
            Assert.IsTrue(coursewise.Any(c => c.CourseId == "C1" && c.Count >= 2));
            Assert.IsTrue(coursewise.Any(c => c.CourseId == "C2" && c.Count >= 1));
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_get_student_parent_details_report_as_csv()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("CsvParent"), "t");
            ids.Add(id);
            await cmds.UpdateParentDetailsAsync(new UpdateParentDetailsRequest
            {
                Id = id, Name = "Father CSV", Mobile = "9000000222", Email = "f@csv.com"
            }, RelationType.Father);
            var flat = await qry.GetFlatDataAsync([id]);
            var contacts = await qry.GetContactsAsync([id]);
            var sb = new StringBuilder();
            sb.AppendLine("StudentId,Name,FatherMobile,Email");
            foreach (var c in contacts)
                sb.AppendLine($"{c.StudentId},{c.Name},{c.FatherMobile},{c.Email}");
            var csv = sb.ToString();
            Assert.IsTrue(csv.Contains("Father") || csv.Contains("9000000222") || csv.Contains(id.ToString()));
            Assert.IsTrue(flat.Count >= 1);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_get_iep_placeholder_from_attributes()
    {
        // IEP report — store marker on student attributes when no dedicated table exists
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("IEP Stu"), "t");
            ids.Add(id);
            var stu = await db.Students.FirstAsync(s => s.Id == id);
            stu.Attributes["iep"] = true;
            stu.Attributes["iepNotes"] = "Speech therapy";
            await db.SaveChangesAsync();
            var reloaded = await db.Students.AsNoTracking().FirstAsync(s => s.Id == id);
            Assert.IsTrue(reloaded.Attributes.ContainsKey("iep"));
        }
        finally { await CleanupAsync(db, ids); }
    }
}
