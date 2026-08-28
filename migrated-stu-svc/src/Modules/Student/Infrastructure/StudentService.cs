using System.Text.Json;
using Ct.BuildingBlocks.Application;
using Ct.Infrastructure.Persistence;
using Ct.Modules.Academics.Application.Students;
using Ct.Modules.Academics.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ct.Modules.Academics.Infrastructure;

public sealed class StudentService(AppDbContext db) : IStudentQueries, IStudentCommands
{
    private static StudentDetails Map(Student student) => new(
        student.Id,
        student.StudentId,
        student.Name,
        student.AdmissionNumber,
        student.ApplicationNumber,
        student.DateOfBirth,
        student.DateOfAdmission,
        student.Email,
        student.Gender,
        student.Mobile,
        student.AadharNumber,
        student.Status,
        student.Enrollments,
        student.Contacts,
        student.Metadata);

    public async Task<StudentDetails> CreateAsync(CreateStudentRequest request, CancellationToken ct)
    {
        var student = new Student
        {
            StudentId = request.StudentId,
            Name = request.Name,
            AdmissionNumber = request.AdmissionNumber,
            ApplicationNumber = request.ApplicationNumber,
            DateOfBirth = request.DateOfBirth,
            DateOfAdmission = request.DateOfAdmission,
            Email = request.Email,
            Gender = request.Gender,
            Mobile = request.Mobile,
            AadharNumber = request.AadharNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Enrollments = JsonSerializer.Serialize(new[]
            {
                new
                {
                    InstId = "",
                    CourseName = request.CourseName,
                    CourseId = request.CourseId,
                    TermName = "",
                    SectionName = "",
                    Branch = ""
                }
            })
        };

        db.Students.Add(student);
        await db.SaveChangesAsync(ct);
        return Map(student);
    }

    public async Task<StudentDetails?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var student = await db.Students.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        return student is null ? null : Map(student);
    }

    public async Task<PagedResult<StudentListItem>> GetAsync(
        string? studentId,
        bool activeOnly,
        bool tcOnly,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 500);

        var query = db.Students.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(studentId))
            query = query.Where(x => x.StudentId == studentId);

        if (activeOnly)
            query = query.Where(x => x.Status == StudentStatus.Active);

        if (tcOnly)
            query = query.Where(x => x.Status == StudentStatus.TransferCertificate);

        var total = await query.CountAsync(ct);
        var rows = await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new StudentListItem(x.Id, x.StudentId, x.Name, x.Status, null, null, null))
            .ToListAsync(ct);

        return new(rows, page, pageSize, total);
    }

    public async Task<IReadOnlyList<StudentListItem>> GetAllAsync(bool includeRte, CancellationToken ct) =>
        await db.Students
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new StudentListItem(x.Id, x.StudentId, x.Name, x.Status, null, null, null))
            .ToListAsync(ct);

    private async Task<bool> SetStatusAsync(Guid id, StudentStatus status, CancellationToken ct)
    {
        var student = await db.Students.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (student is null)
            return false;

        student.Status = status;
        student.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public Task<bool> EnableAsync(Guid id, CancellationToken ct) => SetStatusAsync(id, StudentStatus.Active, ct);
    public Task<bool> DisableAsync(Guid id, CancellationToken ct) => SetStatusAsync(id, StudentStatus.Disabled, ct);
    public Task<bool> TcAsync(Guid id, CancellationToken ct) => SetStatusAsync(id, StudentStatus.TransferCertificate, ct);

    public async Task<StudentDetails?> UpdateAsync(Guid id, UpdateStudentRequest request, CancellationToken ct)
    {
        var student = await db.Students.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (student is null)
            return null;

        student.Name = request.Name;
        student.Email = request.Email;
        student.Mobile = request.Mobile;
        student.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Map(student);
    }
}
