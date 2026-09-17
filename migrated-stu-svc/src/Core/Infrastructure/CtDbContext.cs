using Ct.Modules.Admissions.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ct.Core.Infrastructure;

/// <summary>
/// Single EF Core DbContext for the `ct` schema (spec section 74). Each
/// module registers its own <see cref="Microsoft.EntityFrameworkCore.IEntityTypeConfiguration{TEntity}"/>
/// classes via <c>ApplyConfigurationsFromAssembly</c> instead of this class
/// knowing about module internals directly.
/// </summary>
public class CtDbContext(DbContextOptions<CtDbContext> options) : DbContext(options)
{
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<StudentRelation> StudentRelations => Set<StudentRelation>();
    public DbSet<StudentDomicile> StudentDomiciles => Set<StudentDomicile>();
    public DbSet<StudentEnrollment> StudentEnrollments => Set<StudentEnrollment>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseTerm> CourseTerms => Set<CourseTerm>();
    public DbSet<CourseSection> CourseSections => Set<CourseSection>();
    public DbSet<CourseSubject> CourseSubjects => Set<CourseSubject>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ct");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CtDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        ApplyAuditTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditTimestamps()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Core.Domain.EntityBase>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedOn = entry.Entity.CreatedOn == default ? utcNow : entry.Entity.CreatedOn;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedOn = utcNow;
            }
        }
    }
}
