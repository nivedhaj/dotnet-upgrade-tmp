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
    Task IssueTcAsync(TcRequest request, string? actor, CancellationToken ct = default);
    Task AssignStudentIdAsync(AssignStudentIdRequest request, CancellationToken ct = default);
    Task<bool> PromoteAsync(PromoteStudentRequest request, string? actor, CancellationToken ct = default);
    Task SetHomeAddressAsync(SetHomeAddressRequest request, CancellationToken ct = default);
    Task SetHomePhoneAsync(Guid studentId, string phone, CancellationToken ct = default);
    Task SetPrimaryEmailAsync(Guid studentId, string email, CancellationToken ct = default);
    Task SetPrimaryMobileAsync(Guid studentId, string mobile, CancellationToken ct = default);
    Task SetTagsAsync(Guid studentId, IReadOnlyList<string> tags, CancellationToken ct = default);
    Task SetRollNoAsync(Guid studentId, string instId, string rollNo, CancellationToken ct = default);
    Task UpdateCourseAsync(Guid studentId, string instId, string? courseId, string? courseName, string? termName, string? sectionName, string? branch, CancellationToken ct = default);
    Task UpdateGenderAsync(Guid studentId, string gender, CancellationToken ct = default);
    Task UpdateParentDetailsAsync(UpdateParentDetailsRequest request, RelationType relation, CancellationToken ct = default);
    Task UpdateParentContactAsync(Guid studentId, RelationType relation, ContactType contactType, string value, CancellationToken ct = default);
    Task UpdateAsync(UpdateStudentRequest request, string? actor, CancellationToken ct = default);
    Task UpdateFeesReceivableAsync(FeesReceivableRequest request, CancellationToken ct = default);
    Task UploadPhotoAsync(Guid studentId, string photoUrl, CancellationToken ct = default);
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


    public async Task IssueTcAsync(TcRequest request, string? actor, CancellationToken ct = default)
    {
        var student = await GetTrackedStudentAsync(request.Id, ct);
        student.Status = StudentStatus.Disabled;
        student.Attributes["tc"] = true;
        student.Attributes["tcReason"] = request.Reason;
        student.Attributes["tcDate"] = (request.TcDate ?? DateOnly.FromDateTime(DateTime.UtcNow)).ToString("yyyy-MM-dd");
        if (actor is not null) student.ModifiedBy = actor;
        student.ModifiedOn = DateTime.UtcNow;
        // Tag person with TC
        var withPerson = await db.Students.Include(s => s.Person).ThenInclude(p => p!.Tags)
            .FirstAsync(s => s.Id == request.Id, ct);
        if (withPerson.Person is not null &&
            !withPerson.Person.Tags.Any(t => t.Tag.Equals("TC", StringComparison.OrdinalIgnoreCase)))
        {
            withPerson.Person.Tags.Add(new PersonTag { PersonId = withPerson.PersonId, Tag = "TC" });
        }
        await db.SaveChangesAsync(ct);
    }

    public async Task AssignStudentIdAsync(AssignStudentIdRequest request, CancellationToken ct = default)
    {
        var student = await GetTrackedStudentAsync(request.Id, ct);
        student.StudentId = request.StudentId.Trim();
        await db.SaveChangesAsync(ct);
    }

    public async Task SetHomeAddressAsync(SetHomeAddressRequest request, CancellationToken ct = default)
    {
        var student = await db.Students.Include(s => s.Person).ThenInclude(p => p!.Addresses)
            .FirstOrDefaultAsync(s => s.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Student '{request.Id}' not found.");
        var person = student.Person ?? throw new InvalidOperationException("Student has no person.");
        var home = person.Addresses.FirstOrDefault(a => a.Location == LocationType.Home)
                   ?? person.Addresses.FirstOrDefault();
        if (home is null)
        {
            home = new PersonAddress { PersonId = person.Id, Location = LocationType.Home };
            person.Addresses.Add(home);
            db.Add(home);
        }
        home.Line1 = request.Line1;
        home.Line2 = request.Line2;
        home.City = request.City;
        home.State = request.State;
        home.Pincode = request.PostalCode;
        home.Country = request.Country ?? home.Country;
        home.Status = AddressStatus.Active;
        await db.SaveChangesAsync(ct);
    }

    public async Task SetHomePhoneAsync(Guid studentId, string phone, CancellationToken ct = default)
    {
        var student = await GetTrackedStudentWithPersonAsync(studentId, ct);
        await UpsertPrimaryContactAsync(student.PersonId, ContactType.Phone, phone, ct);
    }

    public async Task UpdateGenderAsync(Guid studentId, string gender, CancellationToken ct = default)
    {
        var student = await db.Students.Include(s => s.Person).FirstOrDefaultAsync(s => s.Id == studentId, ct)
            ?? throw new KeyNotFoundException($"Student '{studentId}' not found.");
        if (student.Person is not null)
            student.Person.Gender = ParseGender(gender);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateParentDetailsAsync(UpdateParentDetailsRequest request, RelationType relation, CancellationToken ct = default)
    {
        var student = await db.Students
            .Include(s => s.Relations).ThenInclude(r => r.Person)
            .FirstOrDefaultAsync(s => s.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Student '{request.Id}' not found.");

        var rel = student.Relations.FirstOrDefault(r => r.RelationType == relation);
        if (rel is null)
        {
            var parent = new Person();
            if (!string.IsNullOrWhiteSpace(request.Name)) parent.SetName(request.Name);
            db.Persons.Add(parent);
            rel = new StudentRelation
            {
                StudentId = student.Id,
                PersonId = parent.Id,
                RelationType = relation,
                Person = parent
            };
            student.Relations.Add(rel);
            db.Add(rel);
        }
        else if (rel.Person is not null && !string.IsNullOrWhiteSpace(request.Name))
        {
            rel.Person.SetName(request.Name);
        }

        if (rel.Person is not null && !string.IsNullOrWhiteSpace(request.Occupation))
        {
            var occ = await db.Set<PersonOccupation>().FirstOrDefaultAsync(o => o.PersonId == rel.PersonId, ct);
            if (occ is null)
            {
                db.Add(new PersonOccupation { PersonId = rel.PersonId, Title = request.Occupation });
            }
            else occ.Title = request.Occupation;
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
            await UpsertPrimaryContactAsync(rel.PersonId, ContactType.Email, request.Email, ct);
        if (!string.IsNullOrWhiteSpace(request.Mobile))
            await UpsertPrimaryContactAsync(rel.PersonId, ContactType.Mobile, request.Mobile, ct);

        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateParentContactAsync(Guid studentId, RelationType relation, ContactType contactType, string value, CancellationToken ct = default)
    {
        var rel = await db.StudentRelations
            .FirstOrDefaultAsync(r => r.StudentId == studentId && r.RelationType == relation, ct)
            ?? throw new KeyNotFoundException($"Relation {relation} not found for student {studentId}.");
        await UpsertPrimaryContactAsync(rel.PersonId, contactType, value, ct);
    }

    public async Task UpdateAsync(UpdateStudentRequest request, string? actor, CancellationToken ct = default)
    {
        var student = await db.Students.Include(s => s.Person).ThenInclude(p => p!.Tags)
            .FirstOrDefaultAsync(s => s.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Student '{request.Id}' not found.");

        if (student.Person is not null)
        {
            if (!string.IsNullOrWhiteSpace(request.Name)) student.Person.SetName(request.Name);
            if (!string.IsNullOrWhiteSpace(request.Gender)) student.Person.Gender = ParseGender(request.Gender);
            if (request.Dob is not null) student.Person.Dob = request.Dob;
        }
        if (!string.IsNullOrWhiteSpace(request.AadharNo)) student.AadharNumber = request.AadharNo;
        if (!string.IsNullOrWhiteSpace(request.StudentId)) student.StudentId = request.StudentId;
        if (!string.IsNullOrWhiteSpace(request.Email))
            await UpsertPrimaryContactAsync(student.PersonId, ContactType.Email, request.Email, ct);
        if (!string.IsNullOrWhiteSpace(request.Mobile))
            await UpsertPrimaryContactAsync(student.PersonId, ContactType.Mobile, request.Mobile, ct);
        if (request.Tags is not null)
            await SetTagsAsync(request.Id, request.Tags, ct);
        if (actor is not null) student.ModifiedBy = actor;
        student.ModifiedOn = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateFeesReceivableAsync(FeesReceivableRequest request, CancellationToken ct = default)
    {
        // Accept Guid or human StudentId
        Student? student = null;
        if (Guid.TryParse(request.Id, out var gid))
            student = await db.Students.FirstOrDefaultAsync(s => s.Id == gid, ct);
        student ??= await db.Students.FirstOrDefaultAsync(s => s.StudentId == request.Id, ct);
        if (student is null) throw new KeyNotFoundException($"Student '{request.Id}' not found.");

        var key = $"feesReceivable:{(request.FeeId ?? "all")}";
        student.Attributes[key] = new Dictionary<string, object?>
        {
            ["feeId"] = request.FeeId,
            ["feeName"] = request.FeeName,
            ["txId"] = request.TxId,
            ["amount"] = request.Amount,
            ["updatedOn"] = DateTimeOffset.UtcNow.ToString("o")
        };
        student.ModifiedOn = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task UploadPhotoAsync(Guid studentId, string photoUrl, CancellationToken ct = default)
    {
        var student = await GetTrackedStudentAsync(studentId, ct);
        student.PhotoUrl = photoUrl;
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
