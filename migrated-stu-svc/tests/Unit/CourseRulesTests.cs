using Ct.Modules.Admissions.Domain;
using Xunit;

namespace Ct.Tests.Unit;

public class CourseRulesTests
{
    private static (Course course, CourseTerm term) CreateCourseWithTerm()
    {
        var course = new Course { Name = "BSc" };
        var term = new CourseTerm { Id = Guid.NewGuid(), CourseId = course.Id, Name = "Year 1" };
        course.Terms.Add(term);
        return (course, term);
    }

    [Fact]
    public void AddSection_RejectsDuplicateNameCaseInsensitively()
    {
        var (course, term) = CreateCourseWithTerm();
        course.AddSection(term, "Section A");

        Assert.Throws<InvalidOperationException>(() => course.AddSection(term, "section a"));
    }

    [Fact]
    public void RenameSection_RejectsEmptyNewName()
    {
        var (course, term) = CreateCourseWithTerm();
        var section = course.AddSection(term, "Section A");

        Assert.Throws<ArgumentException>(() => course.RenameSection(term, section.Id, ""));
    }

    [Fact]
    public void RenameSection_RejectsMissingSection()
    {
        var (course, term) = CreateCourseWithTerm();
        Assert.Throws<InvalidOperationException>(() => course.RenameSection(term, Guid.NewGuid(), "New Name"));
    }

    [Fact]
    public void AddSubject_RejectsEmptyName()
    {
        var (course, term) = CreateCourseWithTerm();
        var section = course.AddSection(term, "Section A");

        Assert.Throws<ArgumentException>(() => course.AddSubject(section, "  ", optional: false));
    }

    [Fact]
    public void AddSubject_RejectsDuplicate()
    {
        var (course, term) = CreateCourseWithTerm();
        var section = course.AddSection(term, "Section A");
        course.AddSubject(section, "Mathematics", optional: false);

        Assert.Throws<InvalidOperationException>(() => course.AddSubject(section, "mathematics", optional: true));
    }

    [Fact]
    public void RenameSubject_OnlyRenamesWhenSubjectExistsAndNameNonEmpty()
    {
        var (course, term) = CreateCourseWithTerm();
        var section = course.AddSection(term, "Section A");
        var subject = course.AddSubject(section, "Mathematics", optional: false);

        course.RenameSubject(section, subject.Id, "");
        Assert.Equal("Mathematics", subject.Name); // unchanged: empty name rejected silently per spec

        course.RenameSubject(section, Guid.NewGuid(), "Physics");
        Assert.Equal("Mathematics", subject.Name); // unchanged: missing subject id rejected silently per spec

        course.RenameSubject(section, subject.Id, "Applied Mathematics");
        Assert.Equal("Applied Mathematics", subject.Name);
    }

    [Fact]
    public void RemoveSubject_ThrowsWhenMissing()
    {
        var (course, term) = CreateCourseWithTerm();
        var section = course.AddSection(term, "Section A");

        Assert.Throws<InvalidOperationException>(() => course.RemoveSubject(section, Guid.NewGuid()));
    }
}
