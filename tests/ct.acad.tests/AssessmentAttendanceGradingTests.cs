using Ct.Acad.Application;
using Ct.Acad.Domain;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Acad.Tests;

[TestClass]
public class AssessmentServiceTests : TestBase
{
    private AssessmentService _svc = null!;
    private Infrastructure.AcadDbContext _db = null!;

    [TestInitialize]
    public void Init()
    {
        _db = CreateDb();
        _svc = new AssessmentService(_db, CreateUser());
    }

    [TestCleanup]
    public void Cleanup() => _db.Dispose();

    [TestMethod]
    [TestCategory("Assessment")]
    public async Task Create_Publish_Unpublish()
    {
        var a = await _svc.CreateAsync(new CreateAssessmentRequest
        {
            Title = "Unit Test 1",
            Subject = "Maths",
            DurationMinutes = 45,
            TotalMarks = 50
        });

        a.Title.Should().Be("Unit Test 1");
        a.Status.Should().Be(nameof(AssessmentStatus.Draft));

        (await _svc.PublishAsync(a.Id)).Should().BeTrue();
        var published = await _svc.GetAllAsync(publishedOnly: true);
        published.Should().ContainSingle(x => x.Id == a.Id);

        (await _svc.UnpublishAsync(a.Id)).Should().BeTrue();
        (await _svc.GetAllAsync(publishedOnly: true)).Should().BeEmpty();
    }

    [TestMethod]
    [TestCategory("Assessment")]
    public async Task Update_and_GetById()
    {
        var a = await _svc.CreateAsync(new CreateAssessmentRequest { Title = "Old" });
        (await _svc.UpdateAsync(a.Id, new CreateAssessmentRequest
        {
            Title = "New Title",
            Subject = "Physics",
            DurationMinutes = 90,
            TotalMarks = 100
        })).Should().BeTrue();

        var loaded = await _svc.GetByIdAsync(a.Id);
        loaded!.Title.Should().Be("New Title");
        loaded.Subject.Should().Be("Physics");
        loaded.DurationMinutes.Should().Be(90);
    }
}

[TestClass]
public class AttendanceServiceTests : TestBase
{
    private AttendanceService _svc = null!;
    private Infrastructure.AcadDbContext _db = null!;

    [TestInitialize]
    public void Init()
    {
        _db = CreateDb();
        _svc = new AttendanceService(_db, CreateUser());
    }

    [TestCleanup]
    public void Cleanup() => _db.Dispose();

    [TestMethod]
    [TestCategory("Attendance")]
    public async Task Post_and_list_absentees()
    {
        var courseId = Guid.NewGuid().ToString();
        var dto = await _svc.PostAsync(new PostAttendanceRequest
        {
            Date = new DateTime(2026, 1, 15),
            CourseId = courseId,
            CourseName = "Std 10",
            Section = "A",
            AbsenteeStudentIds = ["S1", "S2"]
        });

        dto.AbsenteeCount.Should().Be(2);
        dto.Section.Should().Be("A");

        var list = await _svc.GetAbsenteesAsync(new DateTime(2026, 1, 15), courseId);
        list.Should().ContainSingle(x => x.Id == dto.Id);
    }
}

[TestClass]
public class GradingServiceTests : TestBase
{
    private GradingService _svc = null!;
    private Infrastructure.AcadDbContext _db = null!;

    [TestInitialize]
    public void Init()
    {
        _db = CreateDb();
        _svc = new GradingService(_db, CreateUser());
    }

    [TestCleanup]
    public void Cleanup() => _db.Dispose();

    [TestMethod]
    [TestCategory("Grading")]
    public async Task Create_Enable_Disable_GetGrade()
    {
        var g = await _svc.CreateAsync("Default",
        [
            new GradingRuleDto { Grade = "A+", MinPercent = 90, MaxPercent = 100 },
            new GradingRuleDto { Grade = "A", MinPercent = 75, MaxPercent = 89.99m },
            new GradingRuleDto { Grade = "B", MinPercent = 60, MaxPercent = 74.99m },
            new GradingRuleDto { Grade = "F", MinPercent = 0, MaxPercent = 59.99m }
        ]);

        g.Name.Should().Be("Default");
        g.Rules.Should().HaveCount(4);

        var grade = await _svc.GetGradeAsync(g.Id, marks: 85, totalMarks: 100);
        grade.Should().Be("A");

        (await _svc.DisableAsync(g.Id)).Should().BeTrue();
        (await _svc.EnableAsync(g.Id)).Should().BeTrue();
    }

    [TestMethod]
    [TestCategory("Grading")]
    public async Task GetGrade_empty_when_missing_or_zero_total()
    {
        (await _svc.GetGradeAsync(Guid.NewGuid(), 50, 100)).Should().BeEmpty();
        var g = await _svc.CreateAsync("G", [new GradingRuleDto { Grade = "A", MinPercent = 0, MaxPercent = 100 }]);
        (await _svc.GetGradeAsync(g.Id, 50, 0)).Should().BeEmpty();
    }
}
