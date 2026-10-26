using Ct.Acad.Application;
using Ct.Acad.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ct.Acad.Tests;

[TestClass]
public class CourseControllerExtendedTests : TestBase
{
    private Infrastructure.AcadDbContext _db = null!;
    private IAcadUserContext _user = null!;
    private CourseService _courses = null!;
    private CourseExtrasService _extras = null!;
    private CourseController _ctrl = null!;

    [TestInitialize]
    public void Init()
    {
        _db = CreateDb();
        _user = CreateUser();
        _courses = new CourseService(_db, _user);
        _extras = new CourseExtrasService(_db, _user, _courses);
        _ctrl = new CourseController(_courses, _extras);
    }

    [TestCleanup]
    public void Cleanup() => _db.Dispose();

    private async Task<CourseDto> SeedCourse(string name = "Std 10")
    {
        var result = await _ctrl.Post(new CreateCourseRequest { Name = name, EduLevel = "Secondary", SeatsAvailable = 40 }, default);
        return (CourseDto)((OkObjectResult)result).Value!;
    }

    [TestMethod]
    [TestCategory("CourseController")]
    public async Task Get_Post_GetDto_Enable_Disable_UpdateName()
    {
        var c = await SeedCourse("Alpha");
        (await _ctrl.Get(true, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.GetDto(c.Id, null, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.GetById(c.Id, null, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.GetDtoWithData(c.Id, default)).Should().BeOfType<OkObjectResult>();

        (await _ctrl.Disable(new IdRequest { Id = c.Id }, default)).Should().BeOfType<OkResult>();
        (await _ctrl.Enable(new IdRequest { Id = c.Id }, default)).Should().BeOfType<OkResult>();
        (await _ctrl.UpdateName(new NameRequest { Id = c.Id, Name = "Alpha Renamed" }, default)).Should().BeOfType<OkResult>();

        var updated = await _courses.GetByIdAsync(c.Id);
        updated!.Name.Should().Be("Alpha Renamed");
    }

    [TestMethod]
    [TestCategory("CourseController")]
    public async Task Term_Section_Subject_lifecycle()
    {
        var c = await SeedCourse();

        (await _ctrl.NewTerm(new TermSectionRequest { Id = c.Id, TermName = "Annual" }, default))
            .Should().BeOfType<OkResult>();
        (await _ctrl.NewSection(new TermSectionRequest { Id = c.Id, TermName = "Annual", SectionName = "A", Capacity = 30 }, default))
            .Should().BeOfType<OkResult>();
        (await _ctrl.NewSubject(new TermSubjectRequest { Id = c.Id, TermName = "Annual", SubjectName = "Maths", Code = "M", MaxMarks = 100 }, default))
            .Should().BeOfType<OkResult>();

        (await _ctrl.RenameTerm(new RenameTermRequest { Id = c.Id, OldName = "Annual", NewName = "Yearly" }, default))
            .Should().BeOfType<OkResult>();
        (await _ctrl.RenameSection(new RenameSectionRequest { Id = c.Id, TermName = "Yearly", OldSection = "A", NewSection = "A1" }, default))
            .Should().BeOfType<OkResult>();

        (await _ctrl.AddOptionalSubject(new TermSubjectRequest { Id = c.Id, TermName = "Yearly", SubjectName = "Art" }, default))
            .Should().BeOfType<OkResult>();

        (await _ctrl.RemoveSubject(new TermSubjectRequest { Id = c.Id, TermName = "Yearly", SubjectName = "Art" }, default))
            .Should().BeOfType<OkResult>();
        (await _ctrl.RemoveSection(new TermSectionRequest { Id = c.Id, TermName = "Yearly", SectionName = "A1" }, default))
            .Should().BeOfType<OkResult>();
        (await _ctrl.RemoveTerm(new TermSectionRequest { Id = c.Id, TermName = "Yearly" }, default))
            .Should().BeOfType<OkResult>();
    }

    [TestMethod]
    [TestCategory("CourseController")]
    public async Task Update_Clone_Rank_Seats_Facet_Filters()
    {
        var c = await SeedCourse("CloneMe");

        (await _ctrl.UpdateCourse(c.Id, new CreateCourseRequest { Name = "CloneMe v2", SeatsAvailable = 55 }, default))
            .Should().BeOfType<OkObjectResult>();

        var clone = await _ctrl.Clone(new CloneRequest { Id = c.Id, NewName = "CloneMe Copy" }, default);
        clone.Should().BeOfType<OkObjectResult>();

        (await _ctrl.SetRank(new RankRequest { Id = c.Id, Rank = 3 }, default)).Should().BeOfType<OkResult>();
        (await _ctrl.SetSortIndex(new RankRequest { Id = c.Id, Rank = 10 }, default)).Should().BeOfType<OkResult>();
        (await _ctrl.SetSeats(new SeatsRequest { Id = c.Id, Seats = 80 }, default)).Should().BeOfType<OkResult>();

        (await _ctrl.CourseFacet(default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.Facet(default)).Should().BeOfType<OkObjectResult>();
        _ctrl.GetFilters().Should().BeOfType<OkObjectResult>();
        (await _ctrl.GetStudents(c.Id, default)).Should().BeOfType<OkObjectResult>();
        (await _ctrl.ForInstitute(default)).Should().BeOfType<OkObjectResult>();
        _ctrl.Cache().Should().BeOfType<OkObjectResult>();
        _ctrl.MarksCardTemplate().Should().BeOfType<OkObjectResult>();
        _ctrl.SetClassTeacher(new ClassTeacherRequest { Id = c.Id, TermName = "T", SectionName = "A", StaffId = "S1" })
            .Should().BeOfType<OkObjectResult>();
        _ctrl.SetExamSubjectOrder(new { }).Should().BeOfType<OkObjectResult>();
    }
}
