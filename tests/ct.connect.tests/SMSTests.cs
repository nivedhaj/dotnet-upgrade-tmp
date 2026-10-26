using Ct.Connect.Application;
using Ct.Connect.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Connect.Tests;

[TestClass]
public class SMSTests : TestBase
{
    [TestMethod]
    public async Task Can_send_sms_and_persist()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateConnectCmd(db, user);
        var qry = CreateConnectQry(db, user);
        try
        {
            var sms = await cmd.SendSmsAsync(new SendSmsRequest
            {
                Message = "Hello student, your exam is tomorrow.",
                SmsRefId = "ref-001",
                Recipients =
                [
                    new SmsRecipientDto { Name = "A", Mobile = "9876543210" },
                    new SmsRecipientDto { Name = "B", Mobile = "9123456780" }
                ]
            }, senderId: "TESTSEND");

            Assert.IsFalse(string.IsNullOrEmpty(sms.Id));
            Assert.AreEqual(user.InstId, sms.OwnerId);
            Assert.AreEqual(2, sms.Recipients.Count);
            Assert.AreEqual(SMSGatewayEnum.Infini, sms.Gateway);

            var loaded = await qry.GetSmsAsync(sms.Id);
            Assert.IsNotNull(loaded);
            Assert.AreEqual(2, loaded!.Recipients.Count);
            Assert.AreEqual("ref-001", loaded.SmsRefId);
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }

    [TestMethod]
    public async Task Send_sms_normalizes_and_dedupes_mobiles()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateConnectCmd(db, user);
        try
        {
            var sms = await cmd.SendSmsAsync(new SendSmsRequest
            {
                Message = "Test",
                Recipients =
                [
                    new SmsRecipientDto { Mobile = "+91-98765-43210" },
                    new SmsRecipientDto { Mobile = "9876543210" },
                    new SmsRecipientDto { Mobile = "91 9876543210" }
                ]
            });

            Assert.AreEqual(1, sms.Recipients.Count);
            Assert.AreEqual("9876543210", sms.Recipients[0].Mobile);
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }

    [TestMethod]
    public async Task Send_sms_rejects_empty_recipients()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateConnectCmd(db, user);

        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => cmd.SendSmsAsync(new SendSmsRequest
            {
                Message = "Hi",
                Recipients = []
            }));
    }

    [TestMethod]
    public async Task Send_sms_rejects_empty_message()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateConnectCmd(db, user);

        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => cmd.SendSmsAsync(new SendSmsRequest
            {
                Message = "  ",
                Recipients = [new SmsRecipientDto { Mobile = "9876543210" }]
            }));
    }

    [TestMethod]
    public void Get_sms_recipients_from_RecipientPostData()
    {
        using var db = CreateDb();
        var user = CreateUser();
        var qry = CreateConnectQry(db, user);

        var data = new RecipientPostData
        {
            Text = "Hello",
            Recipients = new RecipientsFacet
            {
                Manual = ["9876543210", "invalid", "9123456789", "91-9876543210"]
            }
        };

        var recipients = qry.GetSmsRecipients(data);
        Assert.AreEqual(2, recipients.Count);
        Assert.IsTrue(recipients.All(r => r.Mobile.Length == 10));
    }

    [TestMethod]
    public async Task Can_get_sms_sent_paged()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateConnectCmd(db, user);
        var qry = CreateConnectQry(db, user);
        try
        {
            for (var i = 0; i < 3; i++)
            {
                await cmd.SendSmsAsync(new SendSmsRequest
                {
                    Message = $"Msg {i}",
                    Recipients = [new SmsRecipientDto { Mobile = $"987654321{i}" }]
                });
            }

            var page = await qry.GetSmsSentAsync(page: 1, pageSize: 2);
            Assert.AreEqual(3, page.Total);
            Assert.AreEqual(2, page.Items.Count);
            Assert.AreEqual(1, page.Page);
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }

    [TestMethod]
    public async Task Can_get_sms_delivery_data()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateConnectCmd(db, user);
        var qry = CreateConnectQry(db, user);
        try
        {
            var sms = await cmd.SendSmsAsync(new SendSmsRequest
            {
                Message = "Delivery check",
                Recipients = [new SmsRecipientDto { Mobile = "9876543210" }]
            });

            var data = await qry.GetSmsDeliveryDataAsync(sms.Id);
            Assert.IsNotNull(data);
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }

    [TestMethod]
    public async Task Can_get_sms_text_for_ids()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateConnectCmd(db, user);
        var qry = CreateConnectQry(db, user);
        try
        {
            var a = await cmd.SendSmsAsync(new SendSmsRequest
            {
                Message = "Alpha",
                Recipients = [new SmsRecipientDto { Mobile = "9876543210" }]
            });
            var b = await cmd.SendSmsAsync(new SendSmsRequest
            {
                Message = "Beta",
                Recipients = [new SmsRecipientDto { Mobile = "9123456780" }]
            });

            var texts = await qry.GetSmsTextAsync([a.Id, b.Id]);
            Assert.AreEqual(2, texts.Count);
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }

    [TestMethod]
    public void Sms_credit_helper_calculates_segments()
    {
        Assert.AreEqual(0, SmsCreditHelper.CreditsPerMessage(0));
        Assert.AreEqual(1, SmsCreditHelper.CreditsPerMessage(160));
        Assert.AreEqual(2, SmsCreditHelper.CreditsPerMessage(161));
        Assert.AreEqual(2, SmsCreditHelper.CreditsPerMessage(306));
        Assert.AreEqual(3, SmsCreditHelper.CreditsPerMessage(307));
    }

    [TestMethod]
    public async Task Send_requires_institute_scope()
    {
        await using var db = CreateDb();
        var user = CreateUser(boundByInst: false);
        var cmd = CreateConnectCmd(db, user);

        await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => cmd.SendSmsAsync(new SendSmsRequest
            {
                Message = "Hi",
                Recipients = [new SmsRecipientDto { Mobile = "9876543210" }]
            }));
    }
}
