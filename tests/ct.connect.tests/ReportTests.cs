using Ct.Connect.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Connect.Tests;

[TestClass]
public class ReportTests : TestBase
{
    [TestMethod]
    public async Task Sms_usage_report_aggregates_by_day()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateConnectCmd(db, user);
        var qry = CreateConnectQry(db, user);
        try
        {
            await cmd.SendSmsAsync(new SendSmsRequest
            {
                Message = "Report A",
                Recipients =
                [
                    new SmsRecipientDto { Mobile = "9876543210" },
                    new SmsRecipientDto { Mobile = "9123456780" }
                ]
            });
            await cmd.SendSmsAsync(new SendSmsRequest
            {
                Message = "Report B",
                Recipients = [new SmsRecipientDto { Mobile = "9988776655" }]
            });

            var report = await qry.GetSmsUsageReportAsync();
            Assert.IsNotNull(report);
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }

    [TestMethod]
    public async Task Invalid_mobiles_report_returns_bad_numbers()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateConnectCmd(db, user);
        var qry = CreateConnectQry(db, user);
        try
        {
            var sms = await cmd.SendSmsAsync(new SendSmsRequest
            {
                Message = "X",
                Recipients = [new SmsRecipientDto { Mobile = "9876543210" }]
            });

            var entity = await db.Sms.FindAsync(sms.Id);
            Assert.IsNotNull(entity);
            entity!.Recipients.Add(new Domain.SMSRecipient { Mobile = "123", Name = "Bad" });
            db.Entry(entity).Property(e => e.Recipients).IsModified = true;
            await db.SaveChangesAsync();

            var invalid = await qry.GetInvalidMobilesReportAsync();
            Assert.IsTrue(invalid.Count >= 1);
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }

    [TestMethod]
    public async Task Invalid_emails_report_returns_bad_addresses()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateConnectCmd(db, user);
        var qry = CreateConnectQry(db, user);
        try
        {
            var email = await cmd.SendEmailAsync(new SendEmailRequest
            {
                Subject = "S",
                Message = "M",
                Recipients = ["good@test.com"]
            });

            var entity = await db.Emails.FindAsync(email.Id);
            Assert.IsNotNull(entity);
            entity!.Recipients.Add("not-an-email");
            db.Entry(entity).Property(e => e.Recipients).IsModified = true;
            await db.SaveChangesAsync();

            var invalid = await qry.GetInvalidEmailsReportAsync();
            Assert.IsTrue(invalid.Count >= 1);
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }
}
