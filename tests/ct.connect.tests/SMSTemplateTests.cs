using Ct.Connect.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Connect.Tests;

[TestClass]
public class SMSTemplateTests : TestBase
{
    [TestMethod]
    public async Task Can_request_approval_as_pending()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateConnectCmd(db, user);
        try
        {
            var msg = await cmd.RequestApprovalAsync("Your exam starts at 10 AM", approve: false);

            Assert.AreEqual(SmsMessageStatusEnum.Pending, msg.Status);
            Assert.AreEqual(user.InstId, msg.OwnerId);
            Assert.AreEqual(DefaultUserId, msg.CreatedBy);
            Assert.IsFalse(string.IsNullOrWhiteSpace(msg.Id));
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }

    [TestMethod]
    public async Task Can_create_approved_template()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateConnectCmd(db, user);
        try
        {
            var msg = await cmd.CreateSmsTemplateAsync("Fee due by 15th", approve: true);
            Assert.AreEqual(SmsMessageStatusEnum.Active, msg.Status);
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }

    [TestMethod]
    public async Task Can_enable_disable_template()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateConnectCmd(db, user);
        var qry = CreateConnectQry(db, user);
        try
        {
            var msg = await cmd.CreateSmsTemplateAsync("Template A", approve: true);
            await cmd.DisableSmsTemplateAsync(msg.Id);

            var list = await qry.GetSmsMessagesAsync(activeOnly: false);
            Assert.AreEqual(nameof(SmsMessageStatusEnum.Disabled), list.First(m => m.Id == msg.Id).Status);

            await cmd.EnableSmsTemplateAsync(msg.Id);
            list = await qry.GetSmsMessagesAsync(activeOnly: true);
            Assert.IsTrue(list.Any(m => m.Id == msg.Id));
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }

    [TestMethod]
    public async Task Can_approve_and_disapprove_template()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateConnectCmd(db, user);
        try
        {
            var msg = await cmd.CreateSmsTemplateAsync("Pending text", approve: false);
            await cmd.ApproveSmsTemplateAsync(msg.Id, "Looks good");

            var reloaded = await db.SmsMessages.FindAsync(msg.Id);
            Assert.IsNotNull(reloaded);
            Assert.AreEqual(SmsMessageStatusEnum.Active, reloaded!.Status);

            await cmd.DisapproveSmsTemplateAsync(msg.Id, "Policy violation");
            reloaded = await db.SmsMessages.FindAsync(msg.Id);
            Assert.AreEqual(SmsMessageStatusEnum.Disapproved, reloaded!.Status);
            Assert.AreEqual("Policy violation", reloaded.Reason);
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }

    [TestMethod]
    public async Task Empty_message_throws()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateConnectCmd(db, user);

        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => cmd.RequestApprovalAsync("   "));
    }

    [TestMethod]
    public async Task List_active_only_filters_correctly()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateConnectCmd(db, user);
        var qry = CreateConnectQry(db, user);
        try
        {
            await cmd.CreateSmsTemplateAsync("Active one", approve: true);
            await cmd.CreateSmsTemplateAsync("Pending one", approve: false);

            var active = await qry.GetSmsMessagesAsync(activeOnly: true);
            var all = await qry.GetSmsMessagesAsync(activeOnly: false);

            Assert.AreEqual(1, active.Count);
            Assert.AreEqual(2, all.Count);
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }
}
