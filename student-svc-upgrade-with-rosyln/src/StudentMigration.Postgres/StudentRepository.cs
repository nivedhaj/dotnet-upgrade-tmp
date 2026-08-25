using Microsoft.EntityFrameworkCore;

namespace StudentMigration.Postgres;

public sealed class StudentRepository(AppDbContext db)
{
    public Task<Student?> GetByIdAsync(Guid id, CancellationToken ct = default) => db.Students.Include(x => x.Enrollments).SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<Student?> GetByStudentIdAsync(string studentId, CancellationToken ct = default) => db.Students.Include(x => x.Enrollments).SingleOrDefaultAsync(x => x.StudentId == studentId, ct);
    public async Task<(IReadOnlyList<Student> Data, int Total)> SearchAsync(string? search, int skip, int take, CancellationToken ct = default)
    {
        var q = db.Students.Include(x => x.Enrollments).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search)) q = q.Where(x => EF.Functions.ILike(x.Name, $"%{search}%") || EF.Functions.ILike(x.StudentId, $"%{search}%"));
        var total = await q.CountAsync(ct);
        return (await q.OrderBy(x => x.Name).Skip(skip).Take(take).ToListAsync(ct), total);
    }
    public async Task AddAsync(Student student, CancellationToken ct = default) { db.Students.Add(student); await db.SaveChangesAsync(ct); }
    public async Task SaveAsync(Student student, CancellationToken ct = default) { student.UpdatedAtUtc = DateTime.UtcNow; await db.SaveChangesAsync(ct); }
}
