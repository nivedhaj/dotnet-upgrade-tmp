using Ct.Acad.Application;
using Ct.Acad.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Acad.Tests;

[TestClass]
public class ExamControllerExtendedTests : TestBase
{
    private Infrastructure.AcadDbContext _db = null!;
    private IAcadUserContext _user = null!;
    private ExamService _exams = null!;
    private ExamExtrasService _extras = null!;
    private ExamController _ctrl = null!;

    [TestInitialize]
    public void Init()
    {
        _db = CreateDb();
        _user = CreateUser();
        _exams = new ExamService(_db, _user);
        _extras = new ExamExtrasService(_db, _user, _exams);
        _ctrl = new ExamController(_exams, _extras);
    }

    [TestCleanup]
    public void Cleanup() => _db.Dispose();

    private async Task<ExamDto> SeedExam(string name = "Mid Term")
    {
        var result = await _ctrl.Post(new CreateExamRequest { Name = name, CourseName = "B.Sc", TermName = "Sem1" }, default);
        return (ExamDto)((OkObjectResult)result).Value!;
    }

    [TestMethod]
    [TestCategory("ExamController")]
    public async Task Create_list_get_enable_disable_lock_unlock_rename()
    {
        var e = await SeedExam();
        (await _ctrl.Get(null, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.GetExams(null, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.GetById(e.Id, null, default)).Should().BeOfType<OkObjectResult>();

        (await _ctrl.Enable(new IdRequest { Id = e.Id }, default)).Should().BeOfType<OkResult>();
        (await _ctrl.Rename(new NameRequest { Id = e.Id, Name = "Mid Term v2" }, default)).Should().BeOfType<OkResult>();
        (await _ctrl.Lock(new IdRequest { Id = e.Id }, default)).Should().BeOfType<OkResult>();
        (await _ctrl.Unlock(new IdRequest { Id = e.Id }, default)).Should().BeOfType<OkResult>();
        (await _ctrl.Disable(new IdRequest { Id = e.Id }, default)).Should().BeOfType<OkResult>();
    }

    [TestMethod]
    [TestCategory("ExamController")]
    public async Task Dates_subjects_students_marks_remarks_attendance()
    {
        var e = await SeedExam("Final");
        var date = new DateTime(2026, 4, 1);

        (await _ctrl.SetConductedOn(new ExamDateRequest { Id = e.Id, Date = date }, default)).Should().BeOfType<OkResult>();
        (await _ctrl.SetResultDate(new ExamDateRequest { Id = e.Id, Date = date.AddDays(14) }, default)).Should().BeOfType<OkResult>();
        (await _ctrl.SetEvaluatedOn(new ExamDateRequest { Id = e.Id, Date = date.AddDays(7) }, default)).Should().BeOfType<OkResult>();

        (await _ctrl.AddSubject(new ExamSubjectRequest { Id = e.Id, SubjectName = "Physics", MaxMarks = 100, MinMarks = 35 }, default))
            .Should().BeOfType<OkResult>();

        // add student via service if controller has dedicated endpoint
        await _exams.AddStudentAsync(e.Id, "STU1", "Alice");

        (await _ctrl.SetMarks(new ExamMarksRequest { Id = e.Id, StudentId = "STU1", SubjectName = "Physics", Marks = 92 }, default))
            .Should().BeOfType<OkResult>();
        (await _ctrl.SetDaysPresent(new ExamAttendanceRequest { Id = e.Id, StudentId = "STU1", DaysPresent = 18, DaysWorked = 20 }, default))
            .Should().BeOfType<OkResult>();
        (await _ctrl.SetRemarks(new ExamRemarksRequest { Id = e.Id, StudentId = "STU1", Remarks = "Good" }, default))
            .Should().BeOfType<OkResult>();
        (await _ctrl.SetAttendance(new ExamRemarksRequest { Id = e.Id, StudentId = "STU1", Remarks = "P" }, default))
            .Should().BeOfType<OkResult>();

        (await _ctrl.GetContents(e.Id, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.GetStudents(e.Id, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.GetStudentScores(e.Id, "STU1", default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.Facet(default)).Should().BeOfType<OkObjectResult>();
        _ctrl.GetGradingDetails().Should().BeOfType<OkObjectResult>();

        (await _ctrl.RemoveSubject(new ExamSubjectRequest { Id = e.Id, SubjectName = "Physics" }, default))
            .Should().BeOfType<OkResult>();
    }

    [TestMethod]
    [TestCategory("ExamController")]
    public async Task CopyContents_and_marks_card_aliases()
    {
        var e = await SeedExam("Source");
        await _exams.AddSubjectAsync(e.Id, "Chem", 100, 35);
        await _exams.AddStudentAsync(e.Id, "S1", "Bob");
        await _exams.SetStudentMarksAsync(e.Id, "S1", "Chem", 70);

        // ExamCopyRequest shape
        var copyResult = await _ctrl.CopyContents(new ExamCopyRequest { SourceId = e.Id, NewName = "Source Copy" }, default);
        copyResult.Should().BeOfType<OkObjectResult>();

        (await _ctrl.Evaluate(new ExamMarksRequest { Id = e.Id, StudentId = "S1", SubjectName = "Chem", Marks = 75 }, default))
            .Should().BeOfType<OkResult>();
        (await _ctrl.DaysPresent(new ExamAttendanceRequest { Id = e.Id, StudentId = "S1", DaysPresent = 10 }, default))
            .Should().BeOfType<OkResult>();
        (await _ctrl.ExamRemarks(new ExamRemarksRequest { Id = e.Id, StudentId = "S1", Remarks = "OK" }, default))
            .Should().BeOfType<OkResult>();

        (await _ctrl.ApplyEventsAsync(
        [
            new ExamMarksRequest { Id = e.Id, StudentId = "S1", SubjectName = "Chem", Marks = 80 }
        ], default)).Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    [TestCategory("ExamController")]
    public async Task Locked_exam_rejects_marks_via_service()
    {
        var e = await SeedExam("Locked");
        await _exams.AddSubjectAsync(e.Id, "Bio", 100, 35);
        await _exams.LockAsync(e.Id);

        var act = async () => await _exams.SetStudentMarksAsync(e.Id, "S1", "Bio", 50);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
