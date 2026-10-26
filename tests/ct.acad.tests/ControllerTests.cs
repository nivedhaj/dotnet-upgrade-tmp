using Ct.Acad.Application;
using Ct.Acad.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Acad.Tests;

/// <summary>
/// Controller smoke tests using real services + InMemory EF.
/// </summary>
[TestClass]
public class ControllerTests : TestBase
{
    private Infrastructure.AcadDbContext _db = null!;
    private IAcadUserContext _user = null!;
    private CourseService _courses = null!;
    private ExamService _exams = null!;
    private AssessmentService _assessments = null!;
    private AttendanceService _attendance = null!;
    private GradingService _grading = null!;
    private LmsService _lms = null!;
    private TtService _tt = null!;
    private CalendarService _calendar = null!;
    private AcadReportService _reports = null!;

    [TestInitialize]
    public void Init()
    {
        _db = CreateDb();
        _user = CreateUser();
        _courses = new CourseService(_db, _user);
        _exams = new ExamService(_db, _user);
        _assessments = new AssessmentService(_db, _user);
        _attendance = new AttendanceService(_db, _user);
        _grading = new GradingService(_db, _user);
        _lms = new LmsService(_db, _user);
        _tt = new TtService(_db, _user);
        _calendar = new CalendarService(_db, _user);
        _reports = new AcadReportService(_db, _user, _exams, _courses, _attendance, _calendar);
    }

    [TestCleanup]
    public void Cleanup() => _db.Dispose();

    [TestMethod]
    [TestCategory("Controller")]
    public async Task ExamController_create_enable_disable()
    {
        var ctrl = new ExamController(_exams, new ExamExtrasService(_db, _user, _exams));
        var result = await ctrl.Post(new CreateExamRequest { Name = "Ctrl Exam" }, default);
        result.Should().BeOfType<OkObjectResult>();

        var created = (ExamDto)((OkObjectResult)result).Value!;
        (await ctrl.Enable(new IdRequest { Id = created.Id }, default)).Should().BeOfType<OkResult>();
        (await ctrl.Disable(new IdRequest { Id = created.Id }, default)).Should().BeOfType<OkResult>();
        (await ctrl.Lock(new IdRequest { Id = created.Id }, default)).Should().BeOfType<OkResult>();
        (await ctrl.Unlock(new IdRequest { Id = created.Id }, default)).Should().BeOfType<OkResult>();
    }

    [TestMethod]
    [TestCategory("Controller")]
    public async Task AssessmentController_publish_flow()
    {
        var ctrl = new AssessmentController(_assessments, new AssessmentExtrasService(_db, _user, _assessments));
        var create = await ctrl.CreateAssessment(new CreateAssessmentRequest { Title = "Ctrl Assess" }, default);
        create.Should().BeOfType<OkObjectResult>();
        var dto = (AssessmentDto)((OkObjectResult)create).Value!;

        (await ctrl.PublishAssessment(new IdRequest { Id = dto.Id }, default)).Should().BeOfType<OkResult>();
        (await ctrl.GetPublishedAssessments(default)).Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    [TestCategory("Controller")]
    public async Task AttendanceController_post()
    {
        var ctrl = new AttendanceController(_attendance);
        var result = await ctrl.Post(new PostAttendanceRequest
        {
            Date = DateTime.UtcNow.Date,
            CourseId = "c1",
            CourseName = "Course",
            Section = "A",
            AbsenteeStudentIds = ["s1"]
        }, default);
        result.Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    [TestCategory("Controller")]
    public async Task GradingController_list_and_service_grade()
    {
        var g = await _grading.CreateAsync("Scheme",
        [
            new GradingRuleDto { Grade = "A", MinPercent = 70, MaxPercent = 100 },
            new GradingRuleDto { Grade = "B", MinPercent = 0, MaxPercent = 69.99m }
        ]);
        (await _grading.GetGradeAsync(g.Id, 80, 100)).Should().Be("A");

        var ctrl = new GradingController(_grading);
        (await ctrl.Get(default)).Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    [TestCategory("Controller")]
    public async Task ReportController_course_list()
    {
        await _courses.CreateAsync(new CreateCourseRequest { Name = "Report Course" });
        var extras = new ReportExtrasService(_db, _user, _exams, _reports, new ExamExtrasService(_db, _user, _exams));
        var ctrl = new ReportController(_reports, extras, _assessments);
        (await ctrl.GetCourseListReport(default)).Should().BeOfType<OkObjectResult>();
        (await ctrl.TodaysEvents(default)).Should().BeOfType<OkObjectResult>();
        (await ctrl.HolidayList(default)).Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    [TestCategory("Controller")]
    public async Task CalendarController_create_and_list()
    {
        var extras = new CalendarExtrasService(_db, _user, _calendar);
        var ctrl = new CalendarController(_calendar, extras);
        var result = await ctrl.Post(new CalendarCreateRequest
        {
            Title = "Sports Day",
            Date = DateTime.UtcNow.Date,
            Type = "Event",
            IsHoliday = false
        }, default);
        result.Should().BeOfType<OkObjectResult>();
        (await ctrl.Holidays(default)).Should().BeOfType<OkObjectResult>();
        (await ctrl.Upcoming(30, default)).Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    [TestCategory("Controller")]
    public async Task TTPlannerController_generate()
    {
        var ctrl = new TTPlannerController(_tt);
        var courseId = Guid.NewGuid().ToString();
        await _tt.SetRulesAsync(new TtRulesRequest { CourseId = courseId, CourseName = "C" });
        var result = await ctrl.GenerateTT(new TtGenerateRequest
        {
            CourseId = courseId,
            PeriodCount = 5,
            Subjects = ["Maths"]
        }, default);
        result.Should().BeOfType<OkObjectResult>();
        (await ctrl.Get(courseId, default)).Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    [TestCategory("Controller")]
    public async Task AnonController_published_assessments()
    {
        var a = await _assessments.CreateAsync(new CreateAssessmentRequest { Title = "Public Quiz" });
        await _assessments.PublishAsync(a.Id);

        var ctrl = new AnonController(_assessments, _lms);
        (await ctrl.GetPublishedAssessments(default)).Should().BeOfType<OkObjectResult>();
        (await ctrl.GetTags(default)).Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    [TestCategory("Controller")]
    public void ArtefactController_attendance_template()
    {
        var ctrl = new ArtefactController(new ArtefactService());
        var result = ctrl.StudentAttendanceTemplate();
        result.Should().BeOfType<FileContentResult>()
            .Which.ContentType.Should().Be("text/csv");
    }

    [TestMethod]
    [TestCategory("Controller")]
    public async Task LmsController_create_question()
    {
        var ctrl = new LMSController(_lms, _assessments);
        var result = await ctrl.CreateQuestion(new CreateQuestionRequest
        {
            Text = "Q?",
            Answer = "A",
            Options = ["A", "B"]
        }, default);
        result.Should().BeOfType<OkObjectResult>();
    }
}

