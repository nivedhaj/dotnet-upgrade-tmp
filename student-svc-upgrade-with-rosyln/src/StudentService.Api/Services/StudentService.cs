using Microsoft.EntityFrameworkCore;
using StudentService.Api.Contracts;
using StudentService.Api.Data;
using StudentService.Api.Models;

namespace StudentService.Api.Services;

public sealed class StudentService(StudentDbContext db) : IStudentService
{
    public async Task<StudentResponse?> GetAsync(string id, CancellationToken ct)
    {
        var student = await db.Students
            .AsNoTracking()
            .Include(x => x.Enrollments)
            .SingleOrDefaultAsync(x => x.Id == id, ct);

        return student is null ? null : Map(student);
    }

    public async Task<IReadOnlyList<StudentResponse>> SearchAsync(
        string? instituteId, string? search, bool activeOnly, int skip, int take, CancellationToken ct)
    {
        var query = db.Students
            .AsNoTracking()
            .Include(x => x.Enrollments)
            .AsQueryable();

        if (activeOnly)
            query = query.Where(x => x.Status == StudentStatus.Active);

        if (!string.IsNullOrWhiteSpace(instituteId))
            query = query.Where(x => x.Enrollments.Any(e => e.InstituteId == instituteId));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                EF.Functions.ILike(x.Name, $"%{term}%") ||
                (x.Email != null && EF.Functions.ILike(x.Email, $"%{term}%")) ||
                (x.Mobile != null && EF.Functions.ILike(x.Mobile, $"%{term}%")) ||
                x.Enrollments.Any(e =>
                    e.StudentNumber != null && EF.Functions.ILike(e.StudentNumber, $"%{term}%")));
        }

        var rows = await query
            .OrderBy(x => x.Name)
            .Skip(Math.Max(skip, 0))
            .Take(Math.Clamp(take, 1, 500))
            .ToListAsync(ct);

        return rows.Select(Map).ToList();
    }

    public async Task<StudentResponse> CreateAsync(CreateStudentRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required.");

        if (string.IsNullOrWhiteSpace(request.InstituteId))
            throw new ArgumentException("InstituteId is required.");

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var student = new Student
        {
            Name = request.Name.Trim(),
            Dob = request.Dob,
            Email = request.Email,
            Mobile = request.Mobile,
            AadharNumber = request.AadharNumber,
            Status = StudentStatus.Active
        };

        student.Enrollments.Add(new StudentEnrollment
        {
            InstituteId = request.InstituteId,
            CourseId = request.CourseId,
            CourseName = request.CourseName,
            TermName = request.TermName,
            SectionName = request.SectionName,
            Branch = request.Branch,
            StudentNumber = request.StudentId,
            AdmissionNumber = request.AdmissionNumber,
            ApplicationNumber = request.ApplicationNumber,
            DateOfAdmission = request.Doa,
            Status = EnrollmentStatus.Active
        });

        db.Students.Add(student);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Map(student);
    }

    public async Task SetEnabledAsync(string id, bool enabled, CancellationToken ct)
    {
        var student = await db.Students
            .Include(x => x.Enrollments)
            .SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Student '{id}' was not found.");

        student.Status = enabled ? StudentStatus.Active : StudentStatus.Disabled;

        foreach (var enrollment in student.Enrollments)
            enrollment.Status = enabled ? EnrollmentStatus.Active : EnrollmentStatus.Disabled;

        await db.SaveChangesAsync(ct);
    }

    public async Task PromoteAsync(string id, PromoteStudentRequest request, CancellationToken ct)
    {
        var student = await db.Students
            .Include(x => x.Enrollments)
            .SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Student '{id}' was not found.");

        var baseEnrollment = student.Enrollments.SingleOrDefault(x =>
            x.InstituteId == request.BaseInstituteId &&
            x.CourseId == request.BaseCourseId);

        if (baseEnrollment is null)
            throw new InvalidOperationException("Base enrollment was not found.");

        student.Enrollments.RemoveAll(x => x.InstituteId == request.TargetInstituteId);

        var target = new StudentEnrollment
        {
            InstituteId = request.TargetInstituteId,
            CourseId = request.TargetCourseId,
            CourseName = request.TargetCourseName ?? baseEnrollment.CourseName,
            TermName = request.TargetTerm ?? baseEnrollment.TermName,
            SectionName = request.TargetSection ?? baseEnrollment.SectionName,
            Branch = baseEnrollment.Branch,
            RollNo = baseEnrollment.RollNo,
            StudentNumber = baseEnrollment.StudentNumber,
            AdmissionNumber = baseEnrollment.AdmissionNumber,
            ApplicationNumber = baseEnrollment.ApplicationNumber,
            DateOfAdmission = baseEnrollment.DateOfAdmission,
            Status = EnrollmentStatus.Active,
            Attributes = baseEnrollment.Attributes
        };

        student.Enrollments.Add(target);
        await db.SaveChangesAsync(ct);
    }

    private static StudentResponse Map(Student x) =>
        new(
            x.Id,
            x.Name,
            x.Dob,
            x.Email,
            x.Mobile,
            x.Status.ToString(),
            x.Enrollments.Select(e => new EnrollmentResponse(
                e.InstituteId,
                e.CourseId,
                e.CourseName,
                e.TermName,
                e.SectionName,
                e.Branch,
                e.RollNo,
                e.StudentNumber,
                e.Status.ToString())).ToList());
}
