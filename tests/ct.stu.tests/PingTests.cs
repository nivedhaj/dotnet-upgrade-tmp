using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Stu.Tests;

[TestClass]
public class PingTests : TestBase
{
    [TestMethod]
    public void Can_configure_and_connect()
    {
        Configure();
        Assert.IsFalse(string.IsNullOrWhiteSpace(GetConnectionString()));
        EnsureSchema();
        using var db = CreateDb();
        Assert.IsTrue(db.Database.CanConnect());
        Assert.IsNotNull(CreateCmds(db));
        Assert.IsNotNull(CreateQry(db));
    }
}
