using Ct.Connect.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Connect.Tests;

[TestClass]
public class ConnectTests : TestBase
{
    [TestMethod]
    public async Task Can_send_email_and_load()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateConnectCmd(db, user);
        var qry = CreateConnectQry(db, user);
        try
        {
            var email = await cmd.SendEmailAsync(new SendEmailRequest
            {
                Type = "generic",
                From = "office@test.local",
                Subject = "Fee receipt",
                Message = "<p>Thank you</p>",
                Recipients = ["student@example.com", "parent@example.com"]
            });

            Assert.IsFalse(string.IsNullOrEmpty(email.Id));
            Assert.AreEqual(2, email.Recipients.Count);

            var loaded = await qry.GetEmailAsync(email.Id);
            Assert.IsNotNull(loaded);
            Assert.AreEqual("Fee receipt", loaded!.Subject);
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }

    [TestMethod]
    public async Task Send_email_from_dto_shape()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateConnectCmd(db, user);
        try
        {
            var email = await cmd.SendEmailFromDtoAsync(new CreateEmailDto
            {
                Type = "generic",
                From = "noreply@test.local",
                Subject = "Hello",
                Html = "<b>Hi</b>",
                To = ["a@test.com"],
                Recipients = new RecipientsFacet { Manual = ["b@test.com"] }
            });

            Assert.IsNotNull(email);
            Assert.AreEqual(2, email!.Recipients.Count);
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }

    [TestMethod]
    public async Task Send_email_rejects_no_recipients()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateConnectCmd(db, user);

        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => cmd.SendEmailAsync(new SendEmailRequest
            {
                Subject = "X",
                Message = "Y",
                Recipients = []
            }));
    }

    [TestMethod]
    public void Lookup_validates_mobiles_and_emails()
    {
        using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateConnectCmd(db, user);

        var results = cmd.Lookup(
        [
            "9876543210",
            "+91 91234 56780",
            "bad-mobile",
            "person@example.com",
            "not-an-email"
        ]);

        Assert.AreEqual(5, results.Count);
        Assert.IsTrue(results.Any(r => r.Kind == "mobile" && r.IsValid && r.Normalized == "9876543210"));
        Assert.IsTrue(results.Any(r => r.Kind == "email" && r.IsValid));
        Assert.IsTrue(results.Any(r => !r.IsValid));
    }

    [TestMethod]
    public async Task Can_get_email_sent_paged()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateConnectCmd(db, user);
        var qry = CreateConnectQry(db, user);
        try
        {
            await cmd.SendEmailAsync(new SendEmailRequest
            {
                Subject = "One",
                Message = "Body",
                Recipients = ["a@test.com"]
            });
            await cmd.SendEmailAsync(new SendEmailRequest
            {
                Subject = "Two",
                Message = "Body",
                Recipients = ["b@test.com"]
            });

            var page = await qry.GetEmailSentAsync(page: 1, pageSize: 10);
            Assert.AreEqual(2, page.Total);
            Assert.AreEqual(2, page.Items.Count);
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }

    [TestMethod]
    public async Task Can_get_email_delivery_data()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateConnectCmd(db, user);
        var qry = CreateConnectQry(db, user);
        try
        {
            var email = await cmd.SendEmailAsync(new SendEmailRequest
            {
                Subject = "Delivery",
                Message = "Check",
                Recipients = ["x@test.com"]
            });

            var data = await qry.GetEmailDeliveryDataAsync(email.Id);
            Assert.IsNotNull(data);
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }

    [TestMethod]
    public void Get_sms_sender_id_from_config()
    {
        using var db = CreateDb();
        var user = CreateUser();
        var qry = CreateConnectQry(db, user);

        Assert.AreEqual("TESTSEND", qry.GetSmsSenderId());
    }
}
