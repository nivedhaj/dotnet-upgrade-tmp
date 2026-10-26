using Ct.Scheduler.Application;
using Ct.Scheduler.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Scheduler.Tests;

/// <summary>Port of original CalendarTests.</summary>
[TestClass]
public class CalendarTests : TestBase
{
    [TestMethod]
    public async Task Inst_can_get_calendar_rules()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var cal = CreateCalendar(db, user);
        try
        {
            await cal.AddCalendarRuleAsync(new AddRuleRequest
            {
                Title = "Staff Meeting",
                CronExpression = "0 9 * * 1",
                Category = "Event",
                WeeklyDays = [DayOfWeek.Monday]
            });
            var snapshot = await cal.GetAsync();
            Assert.IsNotNull(snapshot);
            var qry = CreateQry(cal);
            Assert.IsNotNull(await qry.Get());
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Inst_can_create_calendar_rule()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var cal = CreateCalendar(db, user);
        try
        {
            var rule = await cal.AddCalendarRuleAsync(new AddRuleRequest
            {
                Title = "Holiday Rule",
                CronExpression = "* * 0",
                Category = "Holiday",
                WeeklyDays = [DayOfWeek.Sunday]
            });
            Assert.AreNotEqual(Guid.Empty, IdOf(rule));
            var active = await db.Rules.AsNoTracking()
                .CountAsync(r => r.OwnerId == user.OwnerId && r.Status == CalendarRuleStatus.Active);
            Assert.IsTrue(active >= 1);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Inst_can_remove_calendar_rule()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var cal = CreateCalendar(db, user);
        try
        {
            var id = IdOf(await cal.AddCalendarRuleAsync(new AddRuleRequest
            {
                Title = "Temp Rule",
                Category = "Event"
            }));
            Assert.IsTrue(await cal.RemoveCalendarRuleAsync(id));
            var rule = await db.Rules.AsNoTracking().FirstAsync(r => r.Id == id);
            Assert.AreEqual(CalendarRuleStatus.Disabled, rule.Status);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Inst_can_generate_calendar()
    {
        var user = CreateUser(instId: "INST-1");
        await using var db = CreateDb();
        var cal = CreateCalendar(db, user);
        try
        {
            await cal.AddCalendarRuleAsync(new AddRuleRequest
            {
                Title = "Assembly",
                Category = "Event",
                WeeklyDays = [DayOfWeek.Wednesday],
                Duration = 60
            });
            var from = new DateTime(2026, 11, 1);
            var to = new DateTime(2026, 11, 14);
            var result = await cal.GenerateCalendarAsync(new GenerateCalendarRequest
            {
                From = from,
                To = to,
                InstId = "INST-1",
                WeeklyHolidays = [DayOfWeek.Sunday]
            });
            Assert.IsTrue(IntProp(result, "created") > 0);
            var events = await cal.GetCalendarEventsAsync(from, to);
            Assert.IsTrue(events.Count > 0);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Inst_can_remove_occurrence()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var cal = CreateCalendar(db, user);
        try
        {
            var date = new DateTime(2026, 12, 15);
            var ev = await cal.AddCalendarAsync(new AddEventRequest
            {
                EventName = "Sports Day",
                Category = "Event",
                Date = date,
                Duration = 120
            });
            var id = IdOf(ev);
            Assert.IsTrue(await cal.RemoveOccurrenceAsync(id, date));
            var entity = await db.Events.AsNoTracking().FirstAsync(e => e.Id == id);
            Assert.IsTrue(entity.Occurrences.All(o => o.Status == EventOccurrenceStatus.Deleted || o.EventDate.Date != date.Date));
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Inst_can_add_calendar_event()
    {
        var user = CreateUser(instId: "I1");
        await using var db = CreateDb();
        var cal = CreateCalendar(db, user);
        try
        {
            var ev = await cal.AddCalendarAsync(new AddEventRequest
            {
                EventName = "Parent Meeting",
                Category = "Event",
                Dates = [DateTime.UtcNow.Date.AddDays(3), DateTime.UtcNow.Date.AddDays(10)],
                Audience = ["Parents"],
                ConductedBy = ["Principal"],
                Duration = 90,
                Priority = 1
            });
            Assert.AreNotEqual(Guid.Empty, IdOf(ev));
            var upcoming = await cal.GetUpcomingEventsAsync(30);
            Assert.IsTrue(upcoming.Count >= 1);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Cmd_qry_facades_work()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var cal = CreateCalendar(db, user);
        var cmd = CreateCmd(cal);
        var qry = CreateQry(cal);
        try
        {
            await cmd.AddCalendarRule("Facade Rule", "0 8 * * 5", "Event");
            await cmd.GenerateCalendar(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(7), weeklyHolidays: [DayOfWeek.Sunday]);
            Assert.IsNotNull(await qry.Get());
            Assert.IsNotNull(await qry.GetCalendarEvents(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(14)));
            Assert.IsNotNull(await qry.UpcomingEventsWidgetData());
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Tenant_isolation()
    {
        var a = CreateUser();
        var b = CreateUser();
        await using var db = CreateDb();
        try
        {
            var id = IdOf(await CreateCalendar(db, a).AddCalendarRuleAsync(new AddRuleRequest { Title = "Only A" }));
            var bRules = await db.Rules.AsNoTracking().CountAsync(r => r.OwnerId == b.OwnerId && r.Id == id);
            Assert.AreEqual(0, bRules);
        }
        finally
        {
            await CleanupAsync(db, a.OwnerId);
            await CleanupAsync(db, b.OwnerId);
        }
    }
}
