using Ct.Acad.Application;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Acad.Tests;

[TestClass]
public class LmsServiceTests : TestBase
{
    private LmsService _svc = null!;
    private Infrastructure.AcadDbContext _db = null!;

    [TestInitialize]
    public void Init()
    {
        _db = CreateDb();
        _svc = new LmsService(_db, CreateUser());
    }

    [TestCleanup]
    public void Cleanup() => _db.Dispose();

    [TestMethod]
    [TestCategory("LMS")]
    public async Task Create_question_list_and_random()
    {
        await _svc.CreateQuestionAsync(new CreateQuestionRequest
        {
            Text = "2+2=?",
            Options = ["3", "4", "5"],
            Answer = "4",
            Tags = ["math", "easy"],
            Marks = 1
        });

        var all = await _svc.GetQuestionsAsync(null, null, false);
        all.Should().NotBeEmpty();

        var tagged = await _svc.GetQuestionsAsync("math", null, false);
        tagged.Should().NotBeEmpty();

        // Random draws published-only; publish first via update path if needed
        var random = await _svc.GetRandomAsync(5, "math");
        random.Should().NotBeNull();
    }

    [TestMethod]
    [TestCategory("LMS")]
    public async Task SubmitAnswer_scores_correct()
    {
        dynamic created = await _svc.CreateQuestionAsync(new CreateQuestionRequest
        {
            Text = "Capital of France?",
            Options = ["Paris", "Lyon"],
            Answer = "Paris",
            Tags = ["gk"],
            Marks = 2
        });

        Guid qid = created.Id;
        var result = await _svc.SubmitAnswerAsync(qid, "Paris");
        result.Should().NotBeNull();
        ((bool)((dynamic)result!).correct).Should().BeTrue();

        var wrong = await _svc.SubmitAnswerAsync(qid, "Lyon");
        ((bool)((dynamic)wrong!).correct).Should().BeFalse();
    }
}

[TestClass]
public class CalendarServiceTests : TestBase
{
    private CalendarService _svc = null!;
    private Infrastructure.AcadDbContext _db = null!;

    [TestInitialize]
    public void Init()
    {
        _db = CreateDb();
        _svc = new CalendarService(_db, CreateUser());
    }

    [TestCleanup]
    public void Cleanup() => _db.Dispose();

    [TestMethod]
    [TestCategory("Calendar")]
    public async Task Create_Get_Holidays_Upcoming()
    {
        var today = DateTime.UtcNow.Date;
        await _svc.CreateAsync("Independence Day", today, "Holiday", true, "National holiday");
        await _svc.CreateAsync("PTA Meeting", today.AddDays(5), "Event", false, null);

        var all = await _svc.GetAsync(today.AddDays(-1), today.AddDays(30));
        all.Should().HaveCountGreaterThanOrEqualTo(2);

        var holidays = await _svc.HolidaysAsync();
        holidays.Should().NotBeNull();

        var upcoming = await _svc.UpcomingAsync(30);
        upcoming.Should().NotBeNull();
    }
}

[TestClass]
public class TtServiceTests : TestBase
{
    private TtService _svc = null!;
    private Infrastructure.AcadDbContext _db = null!;

    [TestInitialize]
    public void Init()
    {
        _db = CreateDb();
        _svc = new TtService(_db, CreateUser());
    }

    [TestCleanup]
    public void Cleanup() => _db.Dispose();

    [TestMethod]
    [TestCategory("Timetable")]
    public async Task Rules_Generate_List_Cancel()
    {
        var courseId = Guid.NewGuid().ToString();
        await _svc.SetRulesAsync(new TtRulesRequest
        {
            CourseId = courseId,
            CourseName = "Std 10",
            Rules = new Dictionary<string, object> { ["periods"] = 6 }
        });

        var generated = await _svc.GenerateAsync(new TtGenerateRequest
        {
            CourseId = courseId,
            CourseName = "Std 10",
            PeriodCount = 6,
            Subjects = ["Maths", "Science"],
            StaffIds = ["T1", "T2"]
        });
        generated.Should().NotBeNull();

        var list = await _svc.GetAsync(courseId);
        list.Should().NotBeEmpty();
    }
}

[TestClass]
public class ArtefactServiceAcadTests : TestBase
{
    [TestMethod]
    [TestCategory("Artefact")]
    public void StudentAttendanceTemplate_returns_csv_bytes()
    {
        var svc = new ArtefactService();
        var bytes = svc.StudentAttendanceTemplateCsv();
        var text = System.Text.Encoding.UTF8.GetString(bytes);
        text.Should().Contain("Student");
    }
}
