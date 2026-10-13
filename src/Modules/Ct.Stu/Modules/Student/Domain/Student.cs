using Ct.Core.Domain;

namespace Ct.Modules.Admissions.Domain;

/// <summary>
/// Legacy `Student : Person` (spec section 9). Modeled here as Student
/// holding a reference to its own Person record rather than inheritance,
/// which maps far more cleanly onto normalized PostgreSQL tables while
/// preserving the same conceptual shape (spec section 123: Student -> Person
/// -> Contacts/Addresses/Relations/Tags -> Enrollments -> ...).
/// </summary>
public class Student : EntityBase
{
    public Guid PersonId { get; set; }
    public Person? Person { get; set; }

    /// <summary>Legacy human-readable student identifier (distinct from the internal Guid).</summary>
    public string? StudentId { get; set; }

    public string? Attendance { get; set; }
    public string? Category { get; set; }
    public StudentStatus Status { get; set; } = StudentStatus.Active;

    public string? AadharNumber { get; set; }
    public string? Udid { get; set; }
    public string? UserId { get; set; }
    public string? PhotoUrl { get; set; }

    public Dictionary<string, object?> Attributes { get; set; } = new();

    public List<StudentRelation> Relations { get; private set; } = new();
    public List<StudentEnrollment> Enrollments { get; private set; } = new();
    public StudentDomicile? Domicile { get; set; }

    /// <summary>Business rule #1 (spec section 124): a student starts Active.</summary>
    public static Student CreateActive(Guid personId, Guid? requestedId = null)
    {
        return new Student
        {
            Id = requestedId ?? Guid.NewGuid(),
            PersonId = personId,
            Status = StudentStatus.Active
        };
    }

    /// <summary>Business rule #3: Enable sets status Active.</summary>
    public void Enable() => Status = StudentStatus.Active;

    /// <summary>Business rule #2: Disable sets status Disabled.</summary>
    public void Disable() => Status = StudentStatus.Disabled;

    public StudentEnrollment? GetEnrollment(string instId) =>
        Enrollments.FirstOrDefault(e => e.InstId == instId);

    /// <summary>
    /// Legacy `EnrolInInstitute` (spec section 16). Throws if instId is empty,
    /// creates the enrollment if missing, then applies course/term/section/
    /// branch/application/admission/DOA/studentId onto it.
    /// </summary>
    public StudentEnrollment EnrolInInstitute(
        string instId,
        string? courseId,
        string? courseName,
        string? termName,
        string? sectionName,
        string? branch,
        string? applicationNo,
        string? admissionNo,
        DateOnly? doa,
        string? studentInstId)
    {
        if (string.IsNullOrWhiteSpace(instId))
        {
            throw new ArgumentException("Institute id is required.", nameof(instId));
        }

        var enrollment = GetOrCreateEnrollment(instId);

        enrollment.CourseId = courseId;
        enrollment.CourseName = courseName;
        enrollment.TermName = termName;
        enrollment.SectionName = sectionName;
        enrollment.Branch = branch;
        enrollment.ApplicationNo = applicationNo;
        enrollment.AdmissionNo = admissionNo;
        enrollment.Doa = doa;
        enrollment.StudentInstId = studentInstId;

        return enrollment;
    }

    /// <summary>Legacy `AssignInstitute` (spec section 16): create the enrollment if it does not exist.</summary>
    public StudentEnrollment AssignInstitute(string instId) => GetOrCreateEnrollment(instId);

    /// <summary>Legacy `AssignApplicationNo` (spec section 16).</summary>
    public StudentEnrollment AssignApplicationNo(string instId, string applicationNo)
    {
        var enrollment = GetOrCreateEnrollment(instId);
        enrollment.ApplicationNo = applicationNo;
        return enrollment;
    }

    /// <summary>Legacy `AssignAdmissionNo` (spec section 16).</summary>
    public StudentEnrollment AssignAdmissionNo(string instId, string admissionNo)
    {
        var enrollment = GetOrCreateEnrollment(instId);
        enrollment.AdmissionNo = admissionNo;
        return enrollment;
    }

    /// <summary>
    /// Legacy `SetStudentId` (spec section 16 / business rule #9): find the
    /// enrollment by institute and update its StudentId. Unlike the "assign"
    /// operations this does NOT create a missing enrollment implicitly.
    /// </summary>
    public void SetEnrollmentStudentId(string instId, string studentInstId)
    {
        var enrollment = GetEnrollment(instId)
            ?? throw new InvalidOperationException($"No enrollment exists for institute '{instId}'.");
        enrollment.StudentInstId = studentInstId;
    }

    /// <summary>Legacy `SetCourse` / `UpdateCourse` (spec section 16, 40; business rule #10).</summary>
    public StudentEnrollment SetCourse(string instId, string? courseId, string? courseName, string? termName, string? sectionName, string? branch)
    {
        var enrollment = GetOrCreateEnrollment(instId);
        enrollment.CourseId = courseId;
        enrollment.CourseName = courseName;
        enrollment.TermName = termName;
        enrollment.SectionName = sectionName;
        enrollment.Branch = branch;
        return enrollment;
    }

    /// <summary>Legacy `SetRollNo` (spec section 39): updates the current institute's enrollment.</summary>
    public void SetRollNo(string instId, string rollNo)
    {
        var enrollment = GetOrCreateEnrollment(instId);
        enrollment.RollNo = rollNo;
    }

    /// <summary>
    /// Legacy `Promote` (spec section 17; business rule #11-12). Finds the
    /// enrollment matching base institute + base course, clones it onto the
    /// target institute (replacing any existing target-institute enrollment),
    /// and applies term/section overrides when supplied. Returns null if the
    /// base enrollment does not exist, matching legacy "return without
    /// creating" behavior. Caller is responsible for wrapping this in a
    /// PostgreSQL transaction (spec section 77).
    /// </summary>
    public StudentEnrollment? Promote(
        string baseInstId,
        string baseCourseId,
        string? baseTerm,
        string? baseSection,
        string targetInstId,
        string targetCourseId,
        string? targetTerm,
        string? targetSection)
    {
        var baseEnrollment = Enrollments.FirstOrDefault(e =>
            e.InstId == baseInstId && e.CourseId == baseCourseId);

        if (baseEnrollment is null)
        {
            return null;
        }

        var existingTarget = GetEnrollment(targetInstId);
        if (existingTarget is not null)
        {
            Enrollments.Remove(existingTarget);
        }

        var cloned = new StudentEnrollment
        {
            Id = Guid.NewGuid(),
            StudentId = Id,
            InstId = targetInstId,
            CourseId = targetCourseId,
            CourseName = baseEnrollment.CourseName,
            TermName = targetTerm ?? baseEnrollment.TermName,
            SectionName = targetSection ?? baseEnrollment.SectionName,
            Branch = baseEnrollment.Branch,
            StudentInstId = baseEnrollment.StudentInstId,
            ApplicationNo = baseEnrollment.ApplicationNo,
            AdmissionNo = baseEnrollment.AdmissionNo,
            Doa = baseEnrollment.Doa,
            RollNo = baseEnrollment.RollNo,
            Status = EnrollmentStatus.Active
        };

        Enrollments.Add(cloned);
        return cloned;
    }

    public void SetTags(IEnumerable<string> tags, Person person)
    {
        // Legacy `SetTags` replaces the collection wholesale and does not
        // lowercase input (spec section 38).
        person.Tags.Clear();
        foreach (var tag in tags.Distinct())
        {
            person.Tags.Add(new PersonTag { PersonId = person.Id, Tag = tag });
        }
    }

    private StudentEnrollment GetOrCreateEnrollment(string instId)
    {
        var existing = GetEnrollment(instId);
        if (existing is not null)
        {
            return existing;
        }

        var created = new StudentEnrollment
        {
            Id = Guid.NewGuid(),
            StudentId = Id,
            InstId = instId,
            Status = EnrollmentStatus.Active
        };
        Enrollments.Add(created);
        return created;
    }
}
