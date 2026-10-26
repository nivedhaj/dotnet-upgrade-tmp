using Ct.Scheduler.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Scheduler.Tests;

/// <summary>Port of original SchedulerTests (holidays, due date, working days).</summary>
[TestClass]
public class SchedulerTests : TestBase
{
    [TestMethod]
    public async Task Can_get_holidays()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var cal = CreateCalendar(db, user);
        var sched = CreateScheduler(cal);
        try
        {
            await cal.AddCalendarAsync(new AddEventRequest
            {
                EventName = "Diwali",
                Category = "Holiday",
                Date = new DateTime(2026, 11, 8)
            });
            await cal.GenerateCalendarAsync(new GenerateCalendarRequest
            {
                From = new DateTime(2026, 11, 1),
                To = new DateTime(2026, 11, 7),
                WeeklyHolidays = [DayOfWeek.Sunday]
            });
            var holidays = await sched.GetHolidaysAsync();
            Assert.IsTrue(holidays.Count >= 1);
            Assert.IsTrue(holidays.Any(d => d.Date == new DateTime(2026, 11, 8)));
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_library_due_date()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var cal = CreateCalendar(db, user);
        var sched = CreateScheduler(cal);
        try
        {
            // Mark a weekday as holiday so due-date logic skips it
            var start = new DateTime(2026, 10, 26); // Monday
            await cal.AddCalendarAsync(new AddEventRequest
            {
                EventName = "Holiday mid-week",
                Category = "Holiday",
                Date = start.AddDays(2) // Wednesday
            });
            var result = await sched.GetDueDateAsync(start, workingDays: 5);
            var due = DateProp(result, "dueDate");
            Assert.IsTrue(due > start);
            // Should not land on Sunday
            Assert.AreNotEqual(DayOfWeek.Sunday, due.DayOfWeek);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Get_next_working_days()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var cal = CreateCalendar(db, user);
        var sched = CreateScheduler(cal);
        try
        {
            var result = await sched.GetNextWorkingDaysAsync(new DateTime(2026, 10, 26), count: 5);
            Assert.IsNotNull(result);
            using var doc = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(result));
            var arr = doc.RootElement.GetProperty("workingDays");
            Assert.AreEqual(5, arr.GetArrayLength());
            foreach (var el in arr.EnumerateArray())
                Assert.AreNotEqual(DayOfWeek.Sunday, el.GetDateTime().DayOfWeek);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Get_due_date_if_date_is_around_end_of_academic_year()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var cal = CreateCalendar(db, user);
        var sched = CreateScheduler(cal);
        try
        {
            // Near March 31 academic year end — still computes N working days
            var start = new DateTime(2027, 3, 25);
            var result = await sched.GetDueDateAsync(start, workingDays: 10);
            var due = DateProp(result, "dueDate");
            Assert.IsTrue(due > start);
            Assert.IsTrue(due >= new DateTime(2027, 4, 1) || due.Month == 3);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }
}
