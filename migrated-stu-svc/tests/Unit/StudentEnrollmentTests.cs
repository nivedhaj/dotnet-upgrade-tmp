using Ct.Modules.Admissions.Domain;
using Xunit;

namespace Ct.Tests.Unit;

public class StudentEnrollmentTests
{
    [Fact]
    public void CreateActive_StartsInActiveStatus()
    {
        var student = Student.CreateActive(Guid.NewGuid());
        Assert.Equal(StudentStatus.Active, student.Status);
    }

    [Fact]
    public void Disable_ThenEnable_RoundTripsStatus()
    {
        var student = Student.CreateActive(Guid.NewGuid());
        student.Disable();
        Assert.Equal(StudentStatus.Disabled, student.Status);
        student.Enable();
        Assert.Equal(StudentStatus.Active, student.Status);
    }

    [Fact]
    public void EnrolInInstitute_WithEmptyInstId_Throws()
    {
        var student = Student.CreateActive(Guid.NewGuid());
        Assert.Throws<ArgumentException>(() =>
            student.EnrolInInstitute("", null, null, null, null, null, null, null, null, null));
    }

    [Fact]
    public void EnrolInInstitute_CreatesEnrollmentWhenMissing()
    {
        var student = Student.CreateActive(Guid.NewGuid());

        student.EnrolInInstitute("INST1", "C1", "Course One", "Term1", "SecA", "BranchA", "APP1", "ADM1", new DateOnly(2024, 6, 1), "SI-001");

        var enrollment = student.GetEnrollment("INST1");
        Assert.NotNull(enrollment);
        Assert.Equal("C1", enrollment!.CourseId);
        Assert.Equal("SecA", enrollment.SectionName);
        Assert.Equal("ADM1", enrollment.AdmissionNo);
    }

    [Fact]
    public void AssignInstitute_CreatesEnrollmentIfMissing_ButDoesNotDuplicateIfPresent()
    {
        var student = Student.CreateActive(Guid.NewGuid());
        student.AssignInstitute("INST1");
        student.AssignInstitute("INST1");

        Assert.Single(student.Enrollments);
    }

    [Fact]
    public void SetEnrollmentStudentId_ThrowsWhenEnrollmentMissing()
    {
        var student = Student.CreateActive(Guid.NewGuid());
        Assert.Throws<InvalidOperationException>(() => student.SetEnrollmentStudentId("INST1", "SI-1"));
    }

    [Fact]
    public void Promote_ReturnsNull_WhenBaseEnrollmentMissing()
    {
        var student = Student.CreateActive(Guid.NewGuid());
        var result = student.Promote("INST1", "C1", "T1", "SecA", "INST2", "C2", "T2", "SecB");
        Assert.Null(result);
    }

    [Fact]
    public void Promote_ClonesBaseEnrollmentOntoTargetInstitute_ReplacingExistingTargetEnrollment()
    {
        var student = Student.CreateActive(Guid.NewGuid());
        student.EnrolInInstitute("INST1", "C1", "Course One", "T1", "SecA", "BranchA", "APP1", "ADM1", new DateOnly(2024, 6, 1), "SI-001");
        // Pre-existing (stale) enrollment at the target institute must be replaced, not merged.
        student.EnrolInInstitute("INST2", "OLD", "Old Course", "OLD-T", "OLD-S", "OLD-B", null, null, null, null);

        var promoted = student.Promote("INST1", "C1", null, null, "INST2", "C2New", "T2", "SecB");

        Assert.NotNull(promoted);
        Assert.Equal(2, student.Enrollments.Count); // INST1 unchanged + one INST2 enrollment (old one removed)
        var target = student.GetEnrollment("INST2");
        Assert.Equal("C2New", target!.CourseId);
        Assert.Equal("T2", target.TermName);
        Assert.Equal("SecB", target.SectionName);
        Assert.Equal("Course One", target.CourseName); // course name preserved from base per spec section 17 step 10
        Assert.Equal("ADM1", target.AdmissionNo); // carried over from base enrollment
    }

    [Fact]
    public void Promote_RetainsBaseTermAndSection_WhenNotSupplied()
    {
        var student = Student.CreateActive(Guid.NewGuid());
        student.EnrolInInstitute("INST1", "C1", "Course One", "T1", "SecA", "BranchA", null, null, null, null);

        var promoted = student.Promote("INST1", "C1", "T1", "SecA", "INST2", "C2", targetTerm: null, targetSection: null);

        Assert.Equal("T1", promoted!.TermName);
        Assert.Equal("SecA", promoted.SectionName);
    }
}
