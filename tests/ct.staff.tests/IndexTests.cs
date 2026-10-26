using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Staff.Tests;

[TestClass]
public class IndexTests : TestBase
{
    [TestMethod]
    public async Task Can_create_schema_and_query_by_email_index()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var svc = CreateService(db, user);
        try
        {
            await svc.CreateAsync(SampleStaff("Idx", email: "idx@ex.com"));
            Assert.IsNotNull(await svc.GetByEmailAsync("idx@ex.com"));
            var facet = await svc.FacetAsync();
            Assert.IsNotNull(facet);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }
}
