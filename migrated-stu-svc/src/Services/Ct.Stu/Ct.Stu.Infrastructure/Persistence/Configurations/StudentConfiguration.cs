using Ct.Stu.Domain; using Microsoft.EntityFrameworkCore; using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Ct.Stu.Infrastructure.Persistence.Configurations;
public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{ public void Configure(EntityTypeBuilder<Student> b){ b.ToTable("student"); b.HasKey(x=>x.Id); b.Property(x=>x.StudentId).HasColumnName("student_id").HasMaxLength(100).IsRequired(); b.Property(x=>x.Name).HasMaxLength(250).IsRequired(); b.Property(x=>x.Enrollments).HasColumnType("jsonb"); b.Property(x=>x.Contacts).HasColumnType("jsonb"); b.Property(x=>x.Metadata).HasColumnType("jsonb"); b.Property(x=>x.Status).HasConversion<string>(); b.HasIndex(x=>x.StudentId).IsUnique(); b.HasIndex(x=>x.Status); b.HasIndex(x=>x.AdmissionNumber); } }
