using Ct.Acad.Application;
using Ct.Acad.Domain;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Acad.Tests;

[TestClass]
public class ExamServiceTests : TestBase
{
    private ExamService _svc = null!;
    private Infrastructure.AcadDbContext _db = null!;

    [TestInitialize]
    public void Init()
    {
        _db = CreateDb();
        _svc = new ExamService(_db, CreateUser());
    }

    [TestCleanup]
    public void Cleanup() => _db.Dispose();

    [TestMethod]
    [TestCategory("Exam")]
    public async Task Create_and_GetAll()
    {
        var e = await _svc.CreateAsync(new CreateExamRequest
        {
            Name = "Mid Term",
            CourseId = Guid.NewGuid().ToString(),
            CourseName = "B.Sc",
            TermName = "Sem 1"
        });

        e.Name.Should().Be("Mid Term");
        e.Status.Should().Be(nameof(ExamStatus.Active));
        e.Locked.Should().BeFalse();

        var list = await _svc.GetAllAsync();
        list.Should().ContainSingle(x => x.Id == e.Id);
    }

    [TestMethod]
    [TestCategory("Exam")]
    public async Task Enable_Disable_Lock_Unlock_Rename()
    {
        var e = await _svc.CreateAsync(new CreateExamRequest { Name = "Quiz" });

        (await _svc.DisableAsync(e.Id)).Should().BeTrue();
        (await _svc.EnableAsync(e.Id)).Should().BeTrue();
        (await _svc.RenameAsync(e.Id, "Quiz v2")).Should().BeTrue();
        (await _svc.LockAsync(e.Id)).Should().BeTrue();

        var loaded = await _svc.GetByIdAsync(e.Id);
        loaded!.Name.Should().Be("Quiz v2");
        loaded.Locked.Should().BeTrue();

        (await _svc.UnlockAsync(e.Id)).Should().BeTrue();
        (await _svc.GetByIdAsync(e.Id))!.Locked.Should().BeFalse();
    }

    [TestMethod]
    [TestCategory("Exam")]
    public async Task Dates_and_subjects_and_marks()
    {
        var e = await _svc.CreateAsync(new CreateExamRequest { Name = "Final" });
        var when = new DateTime(2026, 3, 15);

        (await _svc.SetConductedOnAsync(e.Id, when)).Should().BeTrue();
        (await _svc.SetResultDateAsync(e.Id, when.AddDays(10))).Should().BeTrue();
        (await _svc.AddSubjectAsync(e.Id, "Physics", 100, 35)).Should().BeTrue();
        (await _svc.AddStudentAsync(e.Id, "STU001", "Alice")).Should().BeTrue();
        (await _svc.SetStudentMarksAsync(e.Id, "STU001", "Physics", 88)).Should().BeTrue();

        var loaded = await _svc.GetByIdAsync(e.Id);
        loaded!.SubjectCount.Should().Be(1);
        loaded.StudentCount.Should().Be(1);
        loaded.ConductedOn.Should().Be(when);
    }

    [TestMethod]
    [TestCategory("Exam")]
    public async Task SetStudentMarks_throws_when_locked()
    {
        var e = await _svc.CreateAsync(new CreateExamRequest { Name = "LockedExam" });
        await _svc.AddSubjectAsync(e.Id, "Chem", 100, 35);
        await _svc.AddStudentAsync(e.Id, "STU1", "Bob");
        await _svc.LockAsync(e.Id);

        var act = async () => await _svc.SetStudentMarksAsync(e.Id, "STU1", "Chem", 50);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*locked*");
    }

    [TestMethod]
    [TestCategory("Exam")]
    public async Task Filter_by_courseId()
    {
        var courseId = Guid.NewGuid().ToString();
        await _svc.CreateAsync(new CreateExamRequest { Name = "A", CourseId = courseId });
        await _svc.CreateAsync(new CreateExamRequest { Name = "B", CourseId = Guid.NewGuid().ToString() });

        var filtered = await _svc.GetAllAsync(courseId);
        filtered.Should().ContainSingle(x => x.Name == "A");
    }
}
