using Ct.Modules.Admissions.Application.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Stu.Tests;

public partial class IndexTests
{
    [TestMethod]
    public async Task Can_create_index_smoke_search_and_facets()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            ids.Add(await cmds.CreateAsync(SampleStudent("IndexA", inst, studentId: "IX-A"), "t"));
            ids.Add(await cmds.CreateAsync(SampleStudent("IndexB", inst, studentId: "IX-B"), "t"));
            var page = await qry.SearchAsync("Index", inst, 1, 10);
            Assert.IsTrue(page.Total >= 2);
            var facets = await qry.GetFacetsAsync(inst);
            Assert.IsTrue(facets.Count >= 1);
        }
        finally { await CleanupAsync(db, ids); }
    }
}
