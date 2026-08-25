using Microsoft.EntityFrameworkCore;

namespace StudentMigration.Postgres;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<StudentEnrollment> StudentEnrollments => Set<StudentEnrollment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Student>(e =>
        {
            e.ToTable("students"); e.HasKey(x => x.Id);
            e.Property(x => x.StudentId).HasMaxLength(100).IsRequired();
            e.Property(x => x.Name).HasMaxLength(300).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
            e.HasIndex(x => x.StudentId);
            e.HasIndex(x => x.Name);
            e.HasMany(x => x.Enrollments).WithOne().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);
        });
        b.Entity<StudentEnrollment>(e =>
        {
            e.ToTable("student_enrollments"); e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.StudentId, x.InstId });
            e.HasIndex(x => new { x.InstId, x.CourseId, x.SectionName });
        });
    }
}
