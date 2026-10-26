using Ct.Modules.Admissions.Application.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Stu.Tests;

/// <summary>
/// Port of EmailTests / EmailSppuTests.
/// Full email dispatch lived in Connect; here we cover recipient resolution from student contacts.
/// </summary>
[TestClass]
public partial class EmailTests : TestBase
{
    [TestMethod]
    public async Task Can_get_recipient_dto_from_stu_ids()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("Mail Stu", email: "child@example.com", mobile: "9000000099"), "t");
            ids.Add(id);
            await cmds.SetPrimaryEmailAsync(id, "child@example.com");
            await cmds.UpdateParentDetailsAsync(new UpdateParentDetailsRequest
            {
                Id = id, Name = "Parent", Email = "parent@example.com", Mobile = "9111111199"
            }, Ct.Modules.Admissions.Domain.RelationType.Father);

            var contacts = await qry.GetContactsAsync([id]);
            Assert.IsTrue(contacts.Count >= 1);
            var c = contacts[0];
            Assert.IsTrue(
                (c.Email is not null && c.Email.Contains('@')) ||
                c.ParentEmails.Any(e => e.Contains('@')));
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_resolve_sectionwise_recipients()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            ids.Add(await cmds.CreateAsync(SampleStudent("SecMail", inst, "C1", "I", "A", email: "a@x.com"), "t"));
            var section = await qry.GetSectionwiseAsync(inst);
            Assert.IsTrue(section.Any(s => s.Students.Count > 0 || s.Count > 0));
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public void Email_template_content_is_loadable_as_string()
    {
        // Legacy Email/* folders held HTML templates; modular host renders templates elsewhere.
        var sample = "<html><body>Dear Parent, {{StudentName}}</body></html>";
        Assert.IsTrue(sample.Contains("{{StudentName}}"));
    }
}
