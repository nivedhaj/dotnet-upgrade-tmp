using Ct.Acad.Application;
using Ct.Acad.Domain;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Acad.Tests;

[TestClass]
public class CourseServiceTests : TestBase
{
    private CourseService _svc = null!;
    private Infrastructure.AcadDbContext _db = null!;

    [TestInitialize]
    public void Init()
    {
        _db = CreateDb();
        _svc = new CourseService(_db, CreateUser());
    }

    [TestCleanup]
    public void Cleanup() => _db.Dispose();

    [TestMethod]
    [TestCategory("Course")]
    public async Task CreateAsync_sets_active_and_owner()
    {
        var dto = await _svc.CreateAsync(new CreateCourseRequest
        {
            Name = "B.Sc CS",
            Branch = "Science",
            EduLevel = "UG",
            SeatsAvailable = 60,
            Program = "Degree"
        });

        dto.Name.Should().Be("B.Sc CS");
        dto.Status.Should().Be(nameof(CourseStatus.Active));
        dto.SeatsAvailable.Should().Be(60);

        var loaded = await _svc.GetByIdAsync(dto.Id);
        loaded.Should().NotBeNull();
        loaded!.EduLevel.Should().Be(nameof(EduLevel.UG));
    }

    [TestMethod]
    [TestCategory("Course")]
    public async Task GetAllAsync_filters_active_only()
    {
        var a = await _svc.CreateAsync(new CreateCourseRequest { Name = "Active Course" });
        var b = await _svc.CreateAsync(new CreateCourseRequest { Name = "To Disable" });
        await _svc.DisableAsync(b.Id);

        var active = await _svc.GetAllAsync(activeOnly: true);
        active.Should().ContainSingle(c => c.Id == a.Id);
        active.Should().NotContain(c => c.Id == b.Id);

        var all = await _svc.GetAllAsync(activeOnly: false);
        all.Should().HaveCount(2);
    }

    [TestMethod]
    [TestCategory("Course")]
    public async Task Enable_Disable_UpdateName()
    {
        var c = await _svc.CreateAsync(new CreateCourseRequest { Name = "X" });
        (await _svc.DisableAsync(c.Id)).Should().BeTrue();
        (await _svc.GetByIdAsync(c.Id))!.Status.Should().Be(nameof(CourseStatus.Disabled));

        (await _svc.EnableAsync(c.Id)).Should().BeTrue();
        (await _svc.UpdateNameAsync(c.Id, "  Renamed  ")).Should().BeTrue();
        (await _svc.GetByIdAsync(c.Id))!.Name.Should().Be("Renamed");
    }

    [TestMethod]
    [TestCategory("Course")]
    public async Task AddTerm_Section_Subject()
    {
        var c = await _svc.CreateAsync(new CreateCourseRequest { Name = "Std 10" });

        (await _svc.AddTermAsync(c.Id, "Annual")).Should().BeTrue();
        (await _svc.AddSectionAsync(c.Id, "Annual", "A", 40)).Should().BeTrue();
        (await _svc.AddSubjectAsync(c.Id, "Annual", "Maths", "MTH", 100)).Should().BeTrue();

        var loaded = await _svc.GetByIdAsync(c.Id);
        loaded!.Terms.Should().ContainSingle(t => t.Name == "Annual");
        loaded.Terms[0].Sections.Should().Contain("A");
        loaded.Terms[0].Subjects.Should().ContainSingle(s => s.Name == "Maths" && s.Code == "MTH");
    }

    [TestMethod]
    [TestCategory("Course")]
    public async Task Operations_return_false_for_unknown_id()
    {
        var id = Guid.NewGuid();
        (await _svc.EnableAsync(id)).Should().BeFalse();
        (await _svc.DisableAsync(id)).Should().BeFalse();
        (await _svc.UpdateNameAsync(id, "x")).Should().BeFalse();
        (await _svc.GetByIdAsync(id)).Should().BeNull();
    }

    [TestMethod]
    [TestCategory("Course")]
    public async Task Owner_isolation()
    {
        var other = new CourseService(_db, CreateUser(ownerId: "other-owner"));
        var theirs = await other.CreateAsync(new CreateCourseRequest { Name = "Other School Course" });

        (await _svc.GetByIdAsync(theirs.Id)).Should().BeNull();
        (await _svc.GetAllAsync(false)).Should().BeEmpty();
    }
}
