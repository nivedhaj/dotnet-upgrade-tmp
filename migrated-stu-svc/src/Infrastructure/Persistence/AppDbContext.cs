using Ct.Modules.Academics.Domain;
using Ct.Modules.Fees.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ct.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Fee> Fees => Set<Fee>();
    public DbSet<FeeTransaction> FeeTransactions => Set<FeeTransaction>();
    public DbSet<StudentFeeSummary> StudentFeeSummaries => Set<StudentFeeSummary>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("stu");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        modelBuilder.Entity<StudentFeeSummary>().HasNoKey().ToView("student_fee_summary_view", "stu");
    }
}
