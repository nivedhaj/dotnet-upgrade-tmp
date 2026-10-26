using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Scheduler.Tests;

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
        Assert.IsNotNull(CreateCalendar(db, user));
    }
}
