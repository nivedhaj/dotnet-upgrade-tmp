using Ct.Connect.Application;
using Ct.Connect.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Connect.Tests;

[TestClass]
public class TopicTests : TestBase
{
    [TestMethod]
    public async Task Can_create_and_list_topics()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateTopicCmd(db, user);
        try
        {
            var created = await cmd.CreateTopicsAsync(
            [
                new CreateTopicRequest
                {
                    Name = "attendance.daily",
                    FriendlyName = "Daily Attendance",
                    Description = "Attendance notifications",
                    Role = "Member",
                    Category = "PrivateToInstitute",
                    Access = "Open",
                    CanUnsubscribe = true,
                    IsSubscriptionAllowed = true,
                    Tags = ["attendance"]
                },
                new CreateTopicRequest
                {
                    Name = "fee.reminder",
                    FriendlyName = "Fee Reminder"
                }
            ]);

            Assert.AreEqual(2, created.Count);
            Assert.IsTrue(created.All(t => t.OwnerId == user.InstId));
            Assert.IsTrue(created.All(t => t.Status == TopicStatusEnum.Active));

            var list = await cmd.GetTopicsAsync(activeOnly: true);
            Assert.AreEqual(2, list.Count);
            Assert.IsTrue(list.Any(t => t.Name == "attendance.daily"));
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }

    [TestMethod]
    public async Task Create_skips_duplicate_name_for_same_owner()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateTopicCmd(db, user);
        try
        {
            await cmd.CreateTopicsAsync([new CreateTopicRequest { Name = "dup.topic" }]);
            var second = await cmd.CreateTopicsAsync([new CreateTopicRequest { Name = "dup.topic" }]);

            Assert.AreEqual(0, second.Count);
            var all = await cmd.GetTopicsAsync();
            Assert.AreEqual(1, all.Count);
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }

    [TestMethod]
    public async Task Can_create_main_topic_and_query_main_topics()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateTopicCmd(db, user);
        try
        {
            var main = await cmd.CreateMainTopicAsync(new CreateTopicRequest
            {
                Name = "academic",
                FriendlyName = "Academic"
            });

            Assert.IsNull(main.MainTopicId);

            await cmd.CreateTopicsAsync(
            [
                new CreateTopicRequest
                {
                    Name = "academic.exam",
                    MainTopicId = main.Id
                }
            ]);

            var mains = await cmd.GetMainTopicsAsync();
            Assert.AreEqual(1, mains.Count);
            Assert.AreEqual("academic", mains[0].Name);
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }

    [TestMethod]
    public async Task Can_enable_disable_topic()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateTopicCmd(db, user);
        try
        {
            var created = await cmd.CreateTopicsAsync([new CreateTopicRequest { Name = "tmp.topic" }]);
            var id = created[0].Id;

            await cmd.DisableAsync(id);
            var disabled = (await cmd.GetTopicsAsync(activeOnly: false)).First(t => t.Id == id);
            Assert.AreEqual(nameof(TopicStatusEnum.Disabled), disabled.Status);

            await cmd.EnableAsync(id);
            var enabled = (await cmd.GetTopicsAsync(activeOnly: true)).First(t => t.Id == id);
            Assert.AreEqual(nameof(TopicStatusEnum.Active), enabled.Status);
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }

    [TestMethod]
    public async Task Can_update_topic_fields()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateTopicCmd(db, user);
        try
        {
            var created = await cmd.CreateTopicsAsync([new CreateTopicRequest { Name = "notif.sms" }]);
            var id = created[0].Id;

            await cmd.UpdateFriendlyNameAsync(id, "SMS Notifications");
            await cmd.UpdateDescriptionAsync(id, "All SMS related topics");
            await cmd.UpdateCategoryAsync(id, "Public");
            await cmd.UpdateRoleAsync(id, "Admin");
            await cmd.UpdateAccessAsync(id, "Restricted");
            await cmd.SetSubscriptionAllowedAsync(id, false);

            var t = (await cmd.GetTopicsAsync()).First(x => x.Id == id);
            Assert.AreEqual("SMS Notifications", t.FriendlyName);
            Assert.AreEqual("All SMS related topics", t.Description);
            Assert.AreEqual(nameof(CategoryEnum.Public), t.Category);
            Assert.AreEqual(nameof(RoleEnum.Admin), t.Role);
            Assert.AreEqual(nameof(AccessEnum.Restricted), t.Access);
            Assert.IsFalse(t.IsSubscriptionAllowed);
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }

    [TestMethod]
    public async Task Can_update_handle_when_valid()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateTopicCmd(db, user);
        try
        {
            var created = await cmd.CreateTopicsAsync([new CreateTopicRequest { Name = "events" }]);
            var id = created[0].Id;

            var updated = await cmd.UpdateHandleAsync(id, "events.main");
            Assert.AreEqual("events.main", updated.Handle);
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }

    [TestMethod]
    public async Task Update_handle_rejects_invalid_prefix()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateTopicCmd(db, user);
        try
        {
            var created = await cmd.CreateTopicsAsync([new CreateTopicRequest { Name = "events" }]);
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => cmd.UpdateHandleAsync(created[0].Id, "other.handle"));
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }

    [TestMethod]
    public async Task Get_handles_returns_active_topic_names()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var cmd = CreateTopicCmd(db, user);
        try
        {
            await cmd.CreateTopicsAsync(
            [
                new CreateTopicRequest { Name = "a.one" },
                new CreateTopicRequest { Name = "b.two" }
            ]);
            var list = await cmd.CreateTopicsAsync([new CreateTopicRequest { Name = "c.three" }]);
            await cmd.DisableAsync(list[0].Id);

            var handles = await cmd.GetHandlesAsync();
            Assert.AreEqual(2, handles.Count);
            Assert.IsFalse(handles.Contains("c.three"));
        }
        finally
        {
            await CleanupAsync(db, user.InstId);
        }
    }

    [TestMethod]
    public async Task Operations_require_institute_scope()
    {
        await using var db = CreateDb();
        var user = CreateUser(boundByInst: false);
        var cmd = CreateTopicCmd(db, user);

        await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => cmd.CreateTopicsAsync([new CreateTopicRequest { Name = "x" }]));
    }
}
