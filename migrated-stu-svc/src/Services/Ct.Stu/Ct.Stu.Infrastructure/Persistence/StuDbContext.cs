using Ct.Stu.Domain; using Microsoft.EntityFrameworkCore;
namespace Ct.Stu.Infrastructure.Persistence;
public sealed class StuDbContext(DbContextOptions<StuDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>(); public DbSet<Fee> Fees => Set<Fee>(); public DbSet<FeeTransaction> FeeTransactions => Set<FeeTransaction>(); public DbSet<StudentFeeSummary> StudentFeeSummaries => Set<StudentFeeSummary>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasDefaultSchema("stu"); b.ApplyConfigurationsFromAssembly(typeof(StuDbContext).Assembly);
        b.Entity<StudentFeeSummary>().HasNoKey().ToView("student_fee_summary_view", "stu");
    }
}
