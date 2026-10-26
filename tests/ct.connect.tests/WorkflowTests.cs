using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Connect.Tests;

[TestClass]
public class WorkflowTests : TestBase
{
    [TestMethod]
    public void Can_init_email_generic_workflow()
    {
        var user = CreateUser(IdBtlCol1718);
        var svc = CreateWorkflowService(user);

        var wf = svc.For("email_generic");
        Assert.IsNotNull(wf);
        Assert.AreEqual("email_generic", wf!.Title);
        Assert.AreEqual(IdBtlCol1718, wf.OwnerId);
        Assert.IsTrue(wf.Jobs.Count >= 4);
        Assert.IsTrue(wf.Jobs.Any(j => j.Title == "email_set_from"));
    }

    [TestMethod]
    public void Can_init_sms_generic_workflow()
    {
        var user = CreateUser();
        var svc = CreateWorkflowService(user);

        var wf = svc.For("sms_generic");
        Assert.IsNotNull(wf);
        Assert.AreEqual("sms_generic", wf!.Title);
        Assert.IsTrue(wf.Jobs.Any(j => j.Title == "sms_send"));
    }

    [TestMethod]
    public void Can_init_email_markscard_workflow()
    {
        var user = CreateUser();
        var svc = CreateWorkflowService(user);

        var wf = svc.For("email_markscard");
        Assert.IsNotNull(wf);
        Assert.AreEqual("email_markscard", wf!.Title);
    }

    [TestMethod]
    public void Unknown_workflow_returns_null()
    {
        var user = CreateUser();
        var svc = CreateWorkflowService(user);

        var wf = svc.For("does_not_exist");
        Assert.IsNull(wf);
    }

    [TestMethod]
    public async Task Can_push_workflow_payload()
    {
        var user = CreateUser();
        var svc = CreateWorkflowService(user);

        using var doc = System.Text.Json.JsonDocument.Parse("""{"title":"email_generic","step":1}""");
        await svc.PushAsync(doc.RootElement);
        // Logging publisher — no exception means success
    }
}
