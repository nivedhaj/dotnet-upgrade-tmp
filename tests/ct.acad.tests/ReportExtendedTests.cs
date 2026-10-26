using Ct.Acad.Application;
using Ct.Acad.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Acad.Tests;

[TestClass]
public class ReportExtendedTests : TestBase
{
    private Infrastructure.AcadDbContext _db = null!;
    private IAcadUserContext _user = null!;
    private CourseService _courses = null!;
    private ExamService _exams = null!;
    private AssessmentService _assessments = null!;
    private AttendanceService _attendance = null!;
    private CalendarService _calendar = null!;
    private AcadReportService _reports = null!;
    private ExamExtrasService _examExtras = null!;
    private ReportExtrasService _extras = null!;
    private ReportController _ctrl = null!;

    [TestInitialize]
    public void Init()
    {
        _db = CreateDb();
        _user = CreateUser();
        _courses = new CourseService(_db, _user);
        _exams = new ExamService(_db, _user);
        _assessments = new AssessmentService(_db, _user);
        _attendance = new AttendanceService(_db, _user);
        _calendar = new CalendarService(_db, _user);
        _reports = new AcadReportService(_db, _user, _exams, _courses, _attendance, _calendar);
        _examExtras = new ExamExtrasService(_db, _user, _exams);
        _extras = new ReportExtrasService(_db, _user, _exams, _reports, _examExtras);
        _ctrl = new ReportController(_reports, _extras, _assessments);
    }

    [TestCleanup]
    public void Cleanup() => _db.Dispose();

    private async Task<Guid> SeedExamWithMarks()
    {
        var exam = await _exams.CreateAsync(new CreateExamRequest { Name = "Report Exam", CourseName = "B.Sc" });
        await _exams.AddSubjectAsync(exam.Id, "Physics", 100, 35);
        await _exams.AddSubjectAsync(exam.Id, "Chem", 100, 35);
        await _exams.AddStudentAsync(exam.Id, "S1", "Alice");
        await _exams.AddStudentAsync(exam.Id, "S2", "Bob");
        await _exams.SetStudentMarksAsync(exam.Id, "S1", "Physics", 90);
        await _exams.SetStudentMarksAsync(exam.Id, "S1", "Chem", 85);
        await _exams.SetStudentMarksAsync(exam.Id, "S2", "Physics", 40);
        await _exams.SetStudentMarksAsync(exam.Id, "S2", "Chem", 55);
        await _exams.SetAttendanceAsync(exam.Id, "S1", "P");
        await _exams.SetAttendanceAsync(exam.Id, "S2", "A");
        return exam.Id;
    }

    [TestMethod]
    [TestCategory("Report")]
    public async Task Course_calendar_and_event_reports()
    {
        await _courses.CreateAsync(new CreateCourseRequest { Name = "Report Course" });
        await _calendar.CreateAsync("Holiday", DateTime.UtcNow.Date, "Holiday", true, null);
        await _attendance.PostAsync(new PostAttendanceRequest
        {
            Date = DateTime.UtcNow.Date,
            CourseId = "c1",
            CourseName = "C",
            Section = "A",
            AbsenteeStudentIds = ["S1"]
        });

        (await _ctrl.GetCourseListReport(default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.TodaysEvents(default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.HolidayList(default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.UpcomingEvents(default)).Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    [TestCategory("Report")]
    public async Task Exam_analytics_reports()
    {
        var examId = await SeedExamWithMarks();
        var body = new IdRequest { Id = examId };

        (await _ctrl.ConsolidatedMarks(body, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.ConsolidatedReport(body, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.RankList(body, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.ResultAtAGlance(body, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.CoursewiseResultAnalysis(body, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.CoursewiseResultAnalysisAsJson(body, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.ResultAnalysis(body, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.ResultAtAGlanceSummary(body, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.SubjectTopScorers(body, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.TopTen(body, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.ExamAttendance(body, default)).Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    [TestCategory("Report")]
    public async Task Failures_fullmarks_gradewise_average()
    {
        var examId = await SeedExamWithMarks();
        var body = new IdRequest { Id = examId };

        // These may live on ReportExtrasService via controller aliases
        (await _ctrl.AllAPlus(body, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.Failures(body, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.ExamwiseFailures(body, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.FullMarks(body, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.GradewiseStatistics(body, null, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.SubjectAverage(body, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.ExamList(default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.ExamListAsJson(default)).Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    [TestCategory("Report")]
    public async Task Student_marks_card_hall_ticket_nodue()
    {
        var examId = await SeedExamWithMarks();

        (await _ctrl.StudentPerformance(new StudentIdBody { StudentId = "S1" }, default))
            .Should().BeOfType<OkObjectResult>();
        var csv = await _ctrl.StudentPerformanceCSV(new StudentIdBody { StudentId = "S1" }, default);
        csv.Should().BeOfType<FileContentResult>();

        (await _ctrl.GetMarksCard(new MarksCardRequest { ExamId = examId, StudentId = "S1" }, default))
            .Should().BeOfType<OkObjectResult>();
        (await _ctrl.HallTicket(new MarksCardRequest { ExamId = examId, StudentId = "S1" }, default))
            .Should().BeOfType<OkObjectResult>();
        (await _ctrl.NoDueCertificate(new StudentIdBody { StudentId = "S1" }, default))
            .Should().BeOfType<OkObjectResult>();
        (await _ctrl.Assessment(default)).Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    [TestCategory("Report")]
    public async Task Report_service_direct_rank_and_glance()
    {
        var examId = await SeedExamWithMarks();
        var ranks = await _reports.RankListAsync(examId);
        ranks.Should().NotBeNull();
        var glance = await _reports.ResultAtAGlanceAsync(examId);
        glance.Should().NotBeNull();
        var top = await _reports.TopTenAsync(examId);
        top.Should().NotBeNull();
        var scorers = await _reports.SubjectTopScorersAsync(examId);
        scorers.Should().NotBeNull();
        var att = await _reports.ExamAttendanceAsync(examId);
        att.Should().NotBeNull();
    }
}
