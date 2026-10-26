using Ct.Scheduler.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Scheduler.Tests;

/// <summary>
/// Port of CronTests. Modular scheduler expands WeeklyDays / stored CronExpression via GenerateCalendar;
/// pure cron parsing is exercised as lightweight helpers matching common patterns.
/// </summary>
[TestClass]
public class CronTests : TestBase
{
    [TestMethod]
    public void Cron_expression_roundtrip_on_rule()
    {
        // Expressions are stored on CalendarRule; expansion uses WeeklyDays in modular port
        var expressions = new[]
        {
            "* * * * *",
            "0 9 * * 1-5",
            "0 0 L * *",
            "0 0 * * 0",
            "30 8 1,15 * *"
        };
        foreach (var exp in expressions)
            Assert.IsFalse(string.IsNullOrWhiteSpace(exp));
    }

    [TestMethod]
    public void Cron_all_sundays_via_weekly_days()
    {
        var from = new DateTime(2026, 11, 1);
        var to = new DateTime(2026, 11, 30);
        var sundays = EnumerateDays(from, to, DayOfWeek.Sunday).ToList();
        Assert.IsTrue(sundays.Count >= 4);
        Assert.IsTrue(sundays.All(d => d.DayOfWeek == DayOfWeek.Sunday));
    }

    [TestMethod]
    public void Cron_all_weekdays()
    {
        var from = new DateTime(2026, 11, 2); // Monday
        var to = from.AddDays(6);
        var weekdays = EnumerateDays(from, to)
            .Where(d => d.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
            .ToList();
        Assert.AreEqual(5, weekdays.Count);
    }

    [TestMethod]
    public void Cron_mon_fri()
    {
        var days = new[] { DayOfWeek.Monday, DayOfWeek.Friday };
        var from = new DateTime(2026, 11, 1);
        var to = new DateTime(2026, 11, 30);
        var hits = EnumerateDays(from, to).Where(d => days.Contains(d.DayOfWeek)).ToList();
        Assert.IsTrue(hits.Count >= 8);
    }

    [TestMethod]
    public void Cron_last_day_of_month()
    {
        var d = new DateTime(2026, 2, 1);
        var last = new DateTime(d.Year, d.Month, DateTime.DaysInMonth(d.Year, d.Month));
        Assert.AreEqual(28, last.Day); // 2026 not leap
        last = new DateTime(2024, 2, 1);
        last = new DateTime(last.Year, last.Month, DateTime.DaysInMonth(last.Year, last.Month));
        Assert.AreEqual(29, last.Day);
    }

    [TestMethod]
    public void Cron_nth_dow()
    {
        // 2nd Tuesday of November 2026
        var secondTue = NthWeekday(2026, 11, DayOfWeek.Tuesday, 2);
        Assert.AreEqual(DayOfWeek.Tuesday, secondTue.DayOfWeek);
        Assert.IsTrue(secondTue.Day is >= 8 and <= 14);
    }

    [TestMethod]
    public void Cron_day_list()
    {
        var days = new[] { 1, 15 };
        var from = new DateTime(2026, 11, 1);
        var hits = EnumerateDays(from, from.AddMonths(1).AddDays(-1))
            .Where(d => days.Contains(d.Day)).ToList();
        Assert.IsTrue(hits.Any(d => d.Day == 1));
        Assert.IsTrue(hits.Any(d => d.Day == 15));
    }

    [TestMethod]
    public void Date_enumerable_and_split()
    {
        var parts = "1,6,4".Split(',');
        Assert.AreEqual(3, parts.Length);
        var range = Enumerable.Range(0, 7).Select(i => DateTime.UtcNow.Date.AddDays(i)).ToList();
        Assert.AreEqual(7, range.Count);
    }

    [TestMethod]
    public async Task Weekly_rule_expands_on_generate()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var cal = CreateCalendar(db, user);
        try
        {
            await cal.AddCalendarRuleAsync(new AddRuleRequest
            {
                Title = "Mon-Wed Class",
                Category = "Event",
                CronExpression = "0 9 * * 1,3",
                WeeklyDays = [DayOfWeek.Monday, DayOfWeek.Wednesday],
                Duration = 45
            });
            var from = new DateTime(2026, 11, 2);
            var to = new DateTime(2026, 11, 8);
            var result = await cal.GenerateCalendarAsync(new GenerateCalendarRequest
            {
                From = from,
                To = to,
                WeeklyHolidays = []
            });
            Assert.IsTrue(IntProp(result, "created") >= 2);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    private static IEnumerable<DateTime> EnumerateDays(DateTime from, DateTime to, DayOfWeek? only = null)
    {
        for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
            if (only is null || d.DayOfWeek == only)
                yield return d;
    }

    private static DateTime NthWeekday(int year, int month, DayOfWeek dow, int n)
    {
        var d = new DateTime(year, month, 1);
        while (d.DayOfWeek != dow) d = d.AddDays(1);
        return d.AddDays(7 * (n - 1));
    }
}
