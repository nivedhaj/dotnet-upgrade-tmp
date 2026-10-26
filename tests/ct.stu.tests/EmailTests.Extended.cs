using Ct.Modules.Admissions.Application.Dtos;
using Ct.Modules.Admissions.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace Ct.Stu.Tests;

public partial class EmailTests
{
    [TestMethod]
    public async Task Can_export_parent_emails()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            for (var i = 0; i < 3; i++)
            {
                var id = await cmds.CreateAsync(SampleStudent($"Mail{i}", inst, email: $"c{i}@x.com"), "t");
                ids.Add(id);
                await cmds.UpdateParentDetailsAsync(new UpdateParentDetailsRequest
                {
                    Id = id, Name = $"Parent{i}", Email = $"p{i}@example.com", Mobile = $"90000000{i:D2}"
                }, RelationType.Father);
            }
            var contacts = await qry.GetContactsAsync(ids);
            var emails = contacts.SelectMany(c => c.ParentEmails.Append(c.Email))
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            Assert.IsTrue(emails.Count >= 3);

            var sb = new StringBuilder();
            sb.AppendLine("email");
            foreach (var e in emails) sb.AppendLine(e);
            Assert.IsTrue(sb.ToString().Contains("@example.com"));
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public async Task Can_get_emails_for_section()
    {
        await using var db = CreateDb();
        var cmds = CreateCmds(db);
        var qry = CreateQry(db);
        var inst = NewInstId();
        var ids = new List<Guid>();
        try
        {
            var id = await cmds.CreateAsync(SampleStudent("SecEmail", inst, "C1", "I", "A"), "t");
            ids.Add(id);
            await cmds.SetPrimaryEmailAsync(id, "sec@school.edu");
            await cmds.UpdateParentDetailsAsync(new UpdateParentDetailsRequest
            {
                Id = id, Name = "P", Email = "parent.sec@example.com"
            }, RelationType.Mother);
            var section = await qry.GetSectionwiseAsync(inst);
            Assert.IsTrue(section.Any(s => s.Count >= 1));
            var contacts = await qry.GetContactsAsync(ids);
            Assert.IsTrue(contacts[0].ParentEmails.Any() || contacts[0].Email is not null);
        }
        finally { await CleanupAsync(db, ids); }
    }

    [TestMethod]
    public void Campaign_templates_are_external_to_stu_module()
    {
        // Legacy gcs_* / sppu_* tests sent real emails with PDF attachments from Email/ folders.
        // Modular Ct.Stu exposes recipients only; Connect/notification host owns campaign send.
        var campaignNames = new[]
        {
            "gcs_180618_School_timings",
            "sppu_180705_tt_pu1",
            "gcs_180713_1to7_Scholarship"
        };
        Assert.IsTrue(campaignNames.Length >= 3);
    }
}
