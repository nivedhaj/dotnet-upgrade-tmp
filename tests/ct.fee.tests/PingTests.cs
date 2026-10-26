using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Fee.Tests;

/// <summary>
/// Port of PingTests (mostly commented out in legacy suite).
/// Smoke: configuration + schema + service construction.
/// </summary>
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
        using var db = CreateDb(user);
        Assert.IsTrue(db.Database.CanConnect());
        var svc = CreateFeeService(db, user);
        Assert.IsNotNull(svc);
    }

    [TestMethod]
    public void Can_resolve_report_service()
    {
        var user = CreateUser();
        using var db = CreateDb(user);
        var reports = CreateReportService(db, user);
        Assert.IsNotNull(reports);
    }
}
