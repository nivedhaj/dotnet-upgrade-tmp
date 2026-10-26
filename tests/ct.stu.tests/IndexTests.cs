using Ct.Modules.Admissions.Application.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Stu.Tests;

/// <summary>Port of Raven IndexTests — EF indexes on student_id / status / enrollments.</summary>
[TestClass]
public partial class IndexTests : TestBase
{
    [TestMethod]
    public async Task Can_create_schema_and_query_by_student_id_index()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("Idx", studentId: "IDX-001"), "t");
            ids.Add(id);
            var page = await qry.SearchAsync("IDX-001", null, 1, 10);
            Assert.IsTrue(page.Items.Any(s => s.StudentId == "IDX-001" || s.Id == id));
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Unique_relation_type_per_student()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("Rel"), "t");
            ids.Add(id);
            await cmds.UpdateParentDetailsAsync(new UpdateParentDetailsRequest { Id = id, Name = "F1" },
                Ct.Modules.Admissions.Domain.RelationType.Father);
            // second update should replace/update same relation, not throw for unique
            await cmds.UpdateParentDetailsAsync(new UpdateParentDetailsRequest { Id = id, Name = "F2" },
                Ct.Modules.Admissions.Domain.RelationType.Father);
            var dto = await CreateQry(db).GetByIdAsync(id);
            Assert.AreEqual("F2", dto!.FatherName);
        }
        finally { await CleanupAsync(db, ids); }
    }
}
