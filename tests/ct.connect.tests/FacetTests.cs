using Ct.Connect.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Connect.Tests;

/// <summary>
/// Facet / contact lookup coverage (replaces original IndexTests / PingTests style smoke checks).
/// </summary>
[TestClass]
public class FacetTests : TestBase
{
    [TestMethod]
    public void Lookup_contact_kind_detection()
    {
        using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateConnectCmd(db, user);

        var results = cmd.Lookup(["9999988888", "admin@college.edu"]);

        var mobile = results.First(r => r.Kind == "mobile");
        var email = results.First(r => r.Kind == "email");

        Assert.IsTrue(mobile.IsValid);
        Assert.AreEqual("9999988888", mobile.Normalized);
        Assert.IsTrue(email.IsValid);
        Assert.AreEqual("admin@college.edu", email.Normalized);
    }

    [TestMethod]
    public void Lookup_rejects_invalid_mobile_length()
    {
        using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateConnectCmd(db, user);

        var results = cmd.Lookup(["12345", "abcdefghij"]);
        Assert.IsTrue(results.All(r => !r.IsValid));
    }
}
