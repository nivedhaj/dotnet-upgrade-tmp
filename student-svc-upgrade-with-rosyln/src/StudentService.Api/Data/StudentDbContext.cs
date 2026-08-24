using Microsoft.EntityFrameworkCore;
using StudentService.Api.Models;

namespace StudentService.Api.Data;

public sealed class StudentDbContext(DbContextOptions<StudentDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<StudentEnrollment> StudentEnrollments => Set<StudentEnrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("students");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(320);
            entity.Property(x => x.Mobile).HasMaxLength(40);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(x => x.Domicile).HasColumnType("jsonb");
            entity.Property(x => x.Documents).HasColumnType("jsonb");
        });

        modelBuilder.Entity<StudentEnrollment>(entity =>
        {
            entity.ToTable("student_enrollments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.StudentId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.InstituteId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CourseId).HasMaxLength(64);
            entity.Property(x => x.StudentNumber).HasMaxLength(100);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(x => x.Attributes).HasColumnType("jsonb");

            entity.HasOne(x => x.Student)
                .WithMany(x => x.Enrollments)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.InstituteId, x.Status });
            entity.HasIndex(x => new { x.InstituteId, x.CourseId, x.SectionName });
        });
    }
}
