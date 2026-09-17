using Ct.Core.Application;
using Ct.Core.Infrastructure;
using Ct.Modules.Admissions.Application.Dtos;
using Ct.Modules.Admissions.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ct.Modules.Admissions.Application.Commands;

/// <summary>
/// Splits the legacy monolithic `StudentCmd` (spec section 34) into a
/// focused set of use cases. Each multi-table write is wrapped in an
/// explicit PostgreSQL transaction per spec section 77.
/// </summary>
public interface IStudentCommandService
{
    Task<Guid> CreateAsync(CreateStudentRequest request, string? actor, CancellationToken ct = default);
    Task EnableAsync(Guid studentId, string? actor, CancellationToken ct = default);
    Task DisableAsync(Guid studentId, string? actor, CancellationToken ct = default);
    Task<bool> PromoteAsync(PromoteStudentRequest request, string? actor, CancellationToken ct = default);
    Task SetPrimaryEmailAsync(Guid studentId, string email, CancellationToken ct = default);
    Task SetPrimaryMobileAsync(Guid studentId, string mobile, CancellationToken ct = default);
    Task SetTagsAsync(Guid studentId, IReadOnlyList<string> tags, CancellationToken ct = default);
    Task SetRollNoAsync(Guid studentId, string instId, string rollNo, CancellationToken ct = default);
    Task UpdateCourseAsync(Guid studentId, string instId, string? courseId, string? courseName, string? termName, string? sectionName, string? branch, CancellationToken ct = default);
}

public class StudentCommandService(CtDbContext db) : IStudentCommandService
{
    /// <summary>Legacy `Create` (spec section 35). Persists Person + Student + optional enrollment atomically.</summary>
    public async Task<Guid> CreateAsync(CreateStudentRequest request, string? actor, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var person = new Person
        {
            Id = Guid.NewGuid(),
            CreatedBy = actor
        };
        person.SetName(request.Name);
        person.Dob = request.Dob;
        person.Email = request.Email;
        person.Mobile = request.Mobile;
        if (Enum.TryParse<Gender>(request.Gender, ignoreCase: true, out var gender))
        {
            person.Gender = gender;
        }
        person.EnsureVirtualId();

        var student = Student.CreateActive(person.Id, request.Id);
        student.AadharNumber = request.AadharNo;
        student.StudentId = request.StudentId;
        student.CreatedBy = actor;

        db.Persons.Add(person);
        db.Students.Add(student);

        // Initialize father/mother/guardian placeholders (spec section 35, step 6)
        // are intentionally NOT created here: the legacy behavior only creates a
        // StudentRelation once a name/contact is actually supplied via the
        // parent-update endpoints (section 37). Creating empty Person rows for
        // absent parents would violate "do not invent new business rules."

        if (!string.IsNullOrWhiteSpace(request.InstId))
        {
            student.EnrolInInstitute(
                instId: request.InstId,
                courseId: request.CourseId,
                courseName: request.CourseName,
                termName: request.TermName,
                sectionName: request.SecName,
                branch: request.Branch,
                applicationNo: request.ApplicationNumber,
                admissionNo: request.AdmissionNumber,
                doa: request.Doa,
                studentInstId: request.StudentId);

            db.StudentEnrollments.AddRange(student.Enrollments);
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return student.Id;
    }

    public async Task EnableAsync(Guid studentId, string? actor, CancellationToken ct = default)
    {
        var student = await GetTrackedStudentAsync(studentId, ct);
        student.Enable();
        student.ModifiedBy = actor;
        await db.SaveChangesAsync(ct);
    }

    public async Task DisableAsync(Guid studentId, string? actor, CancellationToken ct = default)
    {
        var student = await GetTrackedStudentAsync(studentId, ct);
        student.Disable();
        student.ModifiedBy = actor;
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Legacy `Promote` (spec sections 17, 124 rule #11-12) — transactional clone-and-replace.</summary>
    public async Task<bool> PromoteAsync(PromoteStudentRequest request, string? actor, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var student = await db.Students
            .Include(s => s.Enrollments)
            .FirstOrDefaultAsync(s => s.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Student '{request.Id}' not found.");

        var existingTarget = student.GetEnrollment(request.TargetInstId);
        var result = student.Promote(
            request.BaseInstId, request.BaseCourseId, request.BaseTerm, request.BaseSection,
            request.TargetInstId, request.TargetCourseId, request.TargetTerm, request.TargetSection);

        if (result is null)
        {
            // Legacy behavior: return without creating (spec section 17, step 3).
            return false;
        }

        if (existingTarget is not null)
        {
            db.StudentEnrollments.Remove(existingTarget);
        }
        db.StudentEnrollments.Add(result);
        student.ModifiedBy = actor;

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return true;
    }

    public async Task SetPrimaryEmailAsync(Guid studentId, string email, CancellationToken ct = default)
    {
        var student = await GetTrackedStudentWithPersonAsync(studentId, ct);
        await UpsertPrimaryContactAsync(student.PersonId, ContactType.Email, email, ct);
    }

    public async Task SetPrimaryMobileAsync(Guid studentId, string mobile, CancellationToken ct = default)
    {
        var student = await GetTrackedStudentWithPersonAsync(studentId, ct);
        await UpsertPrimaryContactAsync(student.PersonId, ContactType.Mobile, mobile, ct);
    }

    /// <summary>Legacy `SetTags` (spec section 38) — wholesale replace, no implicit lowercasing.</summary>
    public async Task SetTagsAsync(Guid studentId, IReadOnlyList<string> tags, CancellationToken ct = default)
    {
        var student = await db.Students.Include(s => s.Person).ThenInclude(p => p!.Tags)
            .FirstOrDefaultAsync(s => s.Id == studentId, ct)
            ?? throw new KeyNotFoundException($"Student '{studentId}' not found.");

        student.SetTags(tags, student.Person!);
        await db.SaveChangesAsync(ct);
    }

    public async Task SetRollNoAsync(Guid studentId, string instId, string rollNo, CancellationToken ct = default)
    {
        var student = await db.Students.Include(s => s.Enrollments).FirstOrDefaultAsync(s => s.Id == studentId, ct)
            ?? throw new KeyNotFoundException($"Student '{studentId}' not found.");
        student.SetRollNo(instId, rollNo);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateCourseAsync(Guid studentId, string instId, string? courseId, string? courseName, string? termName, string? sectionName, string? branch, CancellationToken ct = default)
    {
        var student = await db.Students.Include(s => s.Enrollments).FirstOrDefaultAsync(s => s.Id == studentId, ct)
            ?? throw new KeyNotFoundException($"Student '{studentId}' not found.");
        student.SetCourse(instId, courseId, courseName, termName, sectionName, branch);
        await db.SaveChangesAsync(ct);
    }

    private async Task<Student> GetTrackedStudentAsync(Guid studentId, CancellationToken ct) =>
        await db.Students.FirstOrDefaultAsync(s => s.Id == studentId, ct)
            ?? throw new KeyNotFoundException($"Student '{studentId}' not found.");

    private async Task<Student> GetTrackedStudentWithPersonAsync(Guid studentId, CancellationToken ct) =>
        await db.Students.Include(s => s.Person).ThenInclude(p => p!.Contacts).FirstOrDefaultAsync(s => s.Id == studentId, ct)
            ?? throw new KeyNotFoundException($"Student '{studentId}' not found.");

    private async Task UpsertPrimaryContactAsync(Guid personId, ContactType type, string info, CancellationToken ct)
    {
        var existing = await db.Set<PersonContact>()
            .FirstOrDefaultAsync(c => c.PersonId == personId && c.ContactType == type && c.IsPrimary, ct);

        if (existing is null)
        {
            db.Add(new PersonContact
            {
                PersonId = personId,
                ContactType = type,
                IsPrimary = true,
                Info = info,
                Location = LocationType.Personal,
                Status = ContactStatus.Active
            });
        }
        else
        {
            existing.Info = info;
        }

        await db.SaveChangesAsync(ct);
    }
}
