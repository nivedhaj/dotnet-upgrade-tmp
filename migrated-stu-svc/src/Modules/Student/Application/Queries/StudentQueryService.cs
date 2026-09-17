using Ct.Core.Application;
using Ct.Core.Infrastructure;
using Ct.Modules.Admissions.Application.Dtos;
using Ct.Modules.Admissions.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ct.Modules.Admissions.Application.Queries;

/// <summary>
/// Splits the legacy monolithic `StudentQry` (spec section 33) into query
/// handlers. All filtering/pagination happens in PostgreSQL — never
/// materialize-then-filter (spec section 76).
/// </summary>
public interface IStudentQueryService
{
    Task<PagedResult<StudentDto>> SearchAsync(string? searchText, string? instId, int page, int pageSize, CancellationToken ct = default);
    Task<StudentDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<StudentDto>> GetByTagAsync(string tag, CancellationToken ct = default);
    Task<IReadOnlyList<StudentFlatDto>> GetFlatDataAsync(IReadOnlyList<Guid> ids, CancellationToken ct = default);
}

public class StudentQueryService(CtDbContext db) : IStudentQueryService
{
    public async Task<PagedResult<StudentDto>> SearchAsync(string? searchText, string? instId, int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = BaseProjection();

        if (!string.IsNullOrWhiteSpace(instId))
        {
            query = query.Where(s => s.Enrollments.Any(e => e.InstId == instId));
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            // Trigram search relies on the pg_trgm GIN index created in
            // database/002_indexes.sql (spec section 69/30) — EF translates
            // ILIKE to a sargable expression Npgsql can use with the index.
            var pattern = $"%{searchText}%";
            query = query.Where(s =>
                EF.Functions.ILike(s.Person!.FirstName, pattern) ||
                EF.Functions.ILike(s.Person!.LastName, pattern) ||
                (s.StudentId != null && EF.Functions.ILike(s.StudentId, pattern)));
        }

        var count = await query.CountAsync(ct);

        var items = await query
            .OrderBy(s => s.Person!.FirstName).ThenBy(s => s.Id) // deterministic ordering (spec section 54)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ProjectionExpression)
            .ToListAsync(ct);

        return PagedResult<StudentDto>.Create(items, count, page, pageSize);
    }

    public async Task<StudentDto?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await BaseProjection()
            .Where(s => s.Id == id)
            .Select(ProjectionExpression)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<StudentDto>> GetByTagAsync(string tag, CancellationToken ct = default) =>
        await BaseProjection()
            .Where(s => s.Person!.Tags.Any(t => t.Tag == tag))
            .Select(ProjectionExpression)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<IReadOnlyList<StudentFlatDto>> GetFlatDataAsync(IReadOnlyList<Guid> ids, CancellationToken ct = default) =>
        await BaseProjection()
            .Where(s => ids.Contains(s.Id))
            .Select(s => new StudentFlatDto
            {
                Id = s.Id,
                Name = s.Person!.Name,
                CourseId = s.Enrollments.Select(e => e.CourseId).FirstOrDefault(),
                CourseName = s.Enrollments.Select(e => e.CourseName).FirstOrDefault(),
                StudentId = s.StudentId,
                Email = s.Person!.Email,
                Dob = s.Person!.Dob,
                Doa = s.Enrollments.Select(e => e.Doa).FirstOrDefault(),
                FatherName = s.Relations.Where(r => r.RelationType == RelationType.Father).Select(r => r.Person!.Name).FirstOrDefault(),
                MotherName = s.Relations.Where(r => r.RelationType == RelationType.Mother).Select(r => r.Person!.Name).FirstOrDefault(),
                GuardianName = s.Relations.Where(r => r.RelationType == RelationType.Guardian).Select(r => r.Person!.Name).FirstOrDefault(),
                AadharNumber = s.AadharNumber,
                RollNo = s.Enrollments.Select(e => e.RollNo).FirstOrDefault(),
                Gender = s.Person!.Gender.ToString(),
                Status = s.Status.ToString(),
                SectionName = s.Enrollments.Select(e => e.SectionName).FirstOrDefault(),
                Tags = s.Person!.Tags.Select(t => t.Tag).ToList()
            })
            .AsNoTracking()
            .ToListAsync(ct);

    private IQueryable<Student> BaseProjection() =>
        db.Students
            .AsNoTracking()
            .Include(s => s.Person!).ThenInclude(p => p.Tags)
            .Include(s => s.Enrollments)
            .Include(s => s.Relations).ThenInclude(r => r.Person);

    private static readonly System.Linq.Expressions.Expression<Func<Student, StudentDto>> ProjectionExpression = s => new StudentDto
    {
        Id = s.Id,
        Name = s.Person!.Name,
        CourseId = s.Enrollments.Select(e => e.CourseId).FirstOrDefault(),
        CourseName = s.Enrollments.Select(e => e.CourseName).FirstOrDefault(),
        StudentId = s.StudentId,
        Email = s.Person!.Email,
        Dob = s.Person!.Dob,
        Doa = s.Enrollments.Select(e => e.Doa).FirstOrDefault(),
        FatherName = s.Relations.Where(r => r.RelationType == RelationType.Father).Select(r => r.Person!.Name).FirstOrDefault(),
        MotherName = s.Relations.Where(r => r.RelationType == RelationType.Mother).Select(r => r.Person!.Name).FirstOrDefault(),
        GuardianName = s.Relations.Where(r => r.RelationType == RelationType.Guardian).Select(r => r.Person!.Name).FirstOrDefault(),
        AadharNumber = s.AadharNumber,
        RollNo = s.Enrollments.Select(e => e.RollNo).FirstOrDefault(),
        Gender = s.Person!.Gender.ToString(),
        Status = s.Status.ToString(),
        SectionName = s.Enrollments.Select(e => e.SectionName).FirstOrDefault(),
        Tags = s.Person!.Tags.Select(t => t.Tag).ToList()
    };
}
