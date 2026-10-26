using Ct.Scheduler.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Scheduler.Tests;

[TestClass]
public class IndexTests : TestBase
{
    [TestMethod]
    public async Task Can_create_schema_and_query_by_owner()
    {
        var user = CreateUser();
        await using var db = CreateDb();
        var cal = CreateCalendar(db, user);
        try
        {
            await cal.AddCalendarRuleAsync(new AddRuleRequest { Title = "Idx Rule", Category = "Event" });
            await cal.AddCalendarAsync(new AddEventRequest
            {
                EventName = "Idx Event",
                Category = "Holiday",
                Date = DateTime.UtcNow.Date
            });
            var snap = await cal.GetAsync();
            Assert.IsNotNull(snap);
            var holidays = await cal.GetHolidayDatesAsync();
            Assert.IsTrue(holidays.Count >= 1);
        }
        finally { await CleanupAsync(db, user.OwnerId); }
    }
}
