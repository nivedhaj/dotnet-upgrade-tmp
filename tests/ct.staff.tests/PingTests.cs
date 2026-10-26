using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Staff.Tests;

[TestClass]
public class PingTests : TestBase
{
    [TestMethod]
    public void Can_configure_and_connect()
    {
        Configure();
        Assert.IsFalse(string.IsNullOrWhiteSpace(GetConnectionString()));
        EnsureSchema();
        var user = CreateUser();
        using var db = CreateDb();
        Assert.IsTrue(db.Database.CanConnect());
        Assert.IsNotNull(CreateService(db, user));
    }
}
