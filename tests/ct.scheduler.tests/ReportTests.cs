using Ct.Scheduler.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Scheduler.Tests;

[TestClass]
public class ReportTests : TestBase
{
    [TestMethod]
    public async Task Can_get_user_count_and_holiday_list()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var cal = CreateCalendar(db, user);
        var reports = CreateReports(cal);
        var reportCmd = CreateReportCmd(cal, reports);
        try
        {
            await cal.AddCalendarAsync(new AddEventRequest
            {
                EventName = "Republic Day",
                Category = "Holiday",
                Date = new DateTime(2027, 1, 26)
            });
            var holidays = await reports.HolidayListAsync();
            Assert.IsNotNull(holidays);

            var calReport = await reports.CalendarReportAsync(new DateTime(2027, 1, 1), new DateTime(2027, 1, 31));
            Assert.IsNotNull(calReport);

            var html = await reportCmd.GetHolidayList();
            Assert.IsNotNull(html);
            var model = await reportCmd.GetCalendarModel(new DateTime(2027, 1, 1), new DateTime(2027, 1, 31));
            Assert.IsNotNull(model);
            var upcoming = await reportCmd.GetUpcomingEventsHtml(60);
            Assert.IsNotNull(upcoming);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }

    [TestMethod]
    public async Task Can_get_calendar_html_report()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var cal = CreateCalendar(db, user);
        var reportCmd = CreateReportCmd(cal, CreateReports(cal));
        try
        {
            await cal.AddCalendarAsync(new AddEventRequest
            {
                EventName = "Annual Day",
                Category = "Event",
                Date = DateTime.UtcNow.Date.AddDays(5)
            });
            var html = await reportCmd.GetCalendar(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(30));
            Assert.IsNotNull(html);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }
}
